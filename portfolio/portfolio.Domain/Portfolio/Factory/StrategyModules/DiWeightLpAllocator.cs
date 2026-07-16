using Google.OrTools.Sat;
using portfolio.Domain.Extensions;
using portfolio.Domain.Models;
using TTM.Shared.Models.PortfolioSimulation;
using LinearExpr = Google.OrTools.Sat.LinearExpr;

namespace portfolio.Domain.Portfolio.Factory.StrategyModules;

/// <summary>
///     Weight-based allocator that optimises purely on <see cref="FunctionSecurityRank.FunctionConvertedRank"/>.
///     <para>
///     Unlike <see cref="DiLegacyLpAllocator"/>, this allocator does NOT use Money, Price or share counts in the
///     optimisation. The decision variables are the portfolio weights themselves, expressed in basis points
///     (0..10000) so they can be modelled with the integer CP-SAT solver. The basis-point integers are divided by
///     10000 when written back onto <see cref="PortfolioValueDto.Weight"/>.
///     </para>
///     <para>
///     A pure "maximise rank" objective with only the budget constraint (sum of weights = 1) is degenerate: the
///     optimum is always 100% in the single highest-ranked security. The diversification constraints below are what
///     make the problem non-trivial. At minimum the per-security max-weight cap must be tight enough to force spread
///     (i.e. <c>maxSecurityWeight * securityCount &gt;= 1</c>) or the model is infeasible.
///     </para>
/// </summary>
public sealed class DiWeightLpAllocator
{
    /// <summary>Weights are modelled as integers in basis points so the integer CP-SAT solver can be used.</summary>
    private const int BasisPoints = 10_000;

    private readonly StrategyInput _input;
    private readonly IReadOnlyList<InternalSecurityRank> _securities;
    private readonly CpSolver _solver = new();
    private readonly List<string> _uniqueCountries;
    private readonly List<string> _uniqueSectors;
    private readonly int _maxSecurityWeightBp;
    private readonly int _minHoldings;

    /// <param name="input">Only <see cref="StrategyInput.SectorWeight"/> and <see cref="StrategyInput.CountryWeight"/> are used; Money/Price fields are ignored.</param>
    /// <param name="securities">Securities to allocate over.</param>
    /// <param name="maxSecurityWeight">Maximum weight any single security may receive (0..1). This is the essential diversification bound.</param>
    /// <param name="minHoldings">Minimum number of securities that must receive a non-zero weight. 0 disables the constraint.</param>
    public DiWeightLpAllocator(
        StrategyInput input,
        IReadOnlyList<InternalSecurityRank> securities,
        double maxSecurityWeight = 0.1,
        int minHoldings = 0)
    {
        _input = input;
        _securities = securities;
        _uniqueSectors = securities.Select(s => s.Security.Sector).Distinct().ToList();
        _uniqueCountries = securities.Select(s => s.Security.Country).Distinct().ToList();
        _maxSecurityWeightBp = (int)Math.Ceiling(maxSecurityWeight * BasisPoints);
        _minHoldings = minHoldings;
    }

    private int NrStocks => _securities.Count;

    /// <summary>Only the budget (sum = 1) and per-security max-weight cap.</summary>
    public List<PortfolioValueDto> AllocateWithOnlySecurityConstraint(out CpSolverStatus resultStatus) =>
        Solve(useSector: false, useCountry: false, useMinHoldings: false, out resultStatus);

    /// <summary>Budget + max-weight cap + per-sector weight caps.</summary>
    public List<PortfolioValueDto> AllocateWithOnlySectorConstraint(out CpSolverStatus resultStatus) =>
        Solve(useSector: true, useCountry: false, useMinHoldings: false, out resultStatus);

    /// <summary>Budget + max-weight cap + sector caps + country caps + minimum number of holdings.</summary>
    public List<PortfolioValueDto> AllocateWithSectorCountrySecuritiesConstraint(out CpSolverStatus resultStatus) =>
        Solve(useSector: true, useCountry: true, useMinHoldings: true, out resultStatus);

    private List<PortfolioValueDto> Solve(bool useSector, bool useCountry, bool useMinHoldings, out CpSolverStatus resultStatus)
    {
        // A fresh model per call so progressively relaxing the constraint set does not accumulate constraints.
        var model = new CpModel();
        var weights = CreateWeightVariables(model);

        SetBudgetConstraint(model, weights);
        SetMaxWeightConstraint(model, weights);
        if (useSector) SetSectorConstraints(model, weights);
        if (useCountry) SetCountryConstraints(model, weights);
        if (useMinHoldings) SetMinHoldingsConstraint(model, weights);

        SetObjectiveFunc(model, weights);

        resultStatus = _solver.Solve(model);

        return MakePortfolioValues(weights);
    }

    private IntVar[] CreateWeightVariables(CpModel model)
    {
        var weights = new IntVar[NrStocks];
        for (var i = 0; i < NrStocks; i++)
            weights[i] = model.NewIntVar(0, BasisPoints, $"W_{i}");

        return weights;
    }

    /// <summary>
    ///     The weights must sum to 1 (10000 basis points). This is the only "budget" the model has — no money involved.
    /// </summary>
    private static void SetBudgetConstraint(CpModel model, IntVar[] weights) =>
        model.Add(LinearExpr.Sum(weights) == BasisPoints);

    /// <summary>
    ///     No single security may exceed <see cref="_maxSecurityWeightBp"/>. Without this the optimum collapses to a
    ///     single security.
    /// </summary>
    private void SetMaxWeightConstraint(CpModel model, IntVar[] weights)
    {
        for (var i = 0; i < NrStocks; i++)
            model.Add(weights[i] <= _maxSecurityWeightBp);
    }

    /// <summary>
    ///     Caps the summed weight of each sector at <see cref="StrategyInput.SectorWeight"/>. Sectors that are absent
    ///     from the dictionary (when any weights are supplied) are capped at 0, mirroring <see cref="DiLegacyLpAllocator"/>.
    ///     If no sector weights are supplied at all the constraint is skipped.
    /// </summary>
    private void SetSectorConstraints(CpModel model, IntVar[] weights)
    {
        if (!_input.SectorWeight.Any())
            return;

        foreach (string sector in _uniqueSectors)
        {
            int capBp = _input.SectorWeight.TryGetValue(sector, out double weight)
                ? (int)Math.Floor(weight * BasisPoints)
                : 0; // weights supplied but not for this sector -> not allowed

            var sectorVariables = SelectVariables(weights, s => s.Security.Sector == sector);
            model.Add(LinearExpr.Sum(sectorVariables) <= capBp);
        }
    }

    /// <summary>
    ///     Caps the summed weight of each country at <see cref="StrategyInput.CountryWeight"/>. Same semantics as
    ///     <see cref="SetSectorConstraints"/>: missing countries are capped at 0, and if no country weights are
    ///     supplied the constraint is skipped.
    /// </summary>
    private void SetCountryConstraints(CpModel model, IntVar[] weights)
    {
        if (!_input.CountryWeight.Any())
            return;

        foreach (string country in _uniqueCountries)
        {
            int capBp = _input.CountryWeight.TryGetValue(country, out double weight)
                ? (int)Math.Floor(weight * BasisPoints)
                : 0; // weights supplied but not for this country -> not allowed

            var countryVariables = SelectVariables(weights, s => s.Security.Country == country);
            model.Add(LinearExpr.Sum(countryVariables) <= capBp);
        }
    }

    /// <summary>
    ///     Forces at least <see cref="_minHoldings"/> securities to carry a non-zero weight using a selection
    ///     indicator per security: the weight is pinned to 0 unless the security is selected, and a selected security
    ///     must carry at least one basis point.
    /// </summary>
    private void SetMinHoldingsConstraint(CpModel model, IntVar[] weights)
    {
        if (_minHoldings <= 0)
            return;

        var indicators = new IntVar[NrStocks];
        for (var i = 0; i < NrStocks; i++)
        {
            indicators[i] = model.NewBoolVar($"B_{i}");
            model.Add(weights[i] <= _maxSecurityWeightBp * indicators[i]); // weight = 0 when not selected
            model.Add(weights[i] >= indicators[i]);                        // weight >= 1bp when selected
        }

        model.Add(LinearExpr.Sum(indicators) >= _minHoldings);
    }

    private List<IntVar> SelectVariables(IntVar[] weights, Func<InternalSecurityRank, bool> predicate)
    {
        var selected = new List<IntVar>();
        for (var i = 0; i < NrStocks; i++)
        {
            if (predicate(_securities[i]))
                selected.Add(weights[i]);
        }

        return selected;
    }

    /// <summary>
    ///     Maximize: R_1*W_1 + R_2*W_2 + ... + R_i*W_i
    ///     Where
    ///     W = Weight (basis points) allocated to security i
    ///     R = Converted rank of security i (higher the better)
    /// </summary>
    private void SetObjectiveFunc(CpModel model, IntVar[] weights)
    {
        var convRank = new List<int>(NrStocks);
        for (var i = 0; i < NrStocks; i++)
            convRank.Add(_securities[i].Rank.FunctionConvertedRank);

        model.Maximize(LinearExpr.WeightedSum(weights, convRank));
    }

    private List<PortfolioValueDto> MakePortfolioValues(IntVar[] weights)
    {
        var portfolioValues = new List<PortfolioValueDto>(NrStocks);
        for (var i = 0; i < NrStocks; i++)
        {
            InternalSecurityRank security = _securities[i];
            long weightBp = _solver.Value(weights[i]);

            portfolioValues.Add(new()
            {
                SecurityId = security.Security.SecurityId,
                Rank = security.Rank.Rank,
                Price = security.Price.MedianPrice(), // informational only; not used by the optimiser
                Amount = 0,                           // share counts are not computed in the weight-based model
                Weight = weightBp / (double)BasisPoints
            });
        }

        return portfolioValues;
    }
}
