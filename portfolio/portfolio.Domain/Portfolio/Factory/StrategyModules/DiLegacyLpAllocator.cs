using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;
using portfolio.Domain.Extensions;
using portfolio.Domain.Models;
using TTM.Shared.Models.PortfolioSimulation;
using Constraint = Google.OrTools.LinearSolver.Constraint;
using LinearExpr = Google.OrTools.Sat.LinearExpr;

namespace portfolio.Domain.Portfolio.Factory.StrategyModules;

public sealed class DiLegacyLpAllocator
{
    private readonly StrategyInput _input;
    private readonly IReadOnlyList<InternalSecurityRank> _securities;
    private readonly CpSolver solver = new CpSolver();
    private readonly CpModel _model = new CpModel();
    private readonly List<string> _uniqueCountries;
    private readonly List<string> _uniqueSectors;
    
    public DiLegacyLpAllocator(StrategyInput input, IReadOnlyList<InternalSecurityRank> securities)
    {
        _input = input;
        _securities = securities;
        X_NrStocks = GetNrStocksOptimVariables();
        _uniqueSectors = _securities.Select(s => s.Security.Sector).Distinct().ToList();
        _uniqueCountries = _securities.Select(s => s.Security.Country).Distinct().ToList();
    }

    private int NrStocks => _securities.Count;
    private IntVar[] X_NrStocks { get; set; }

    private IntVar[] GetNrStocksOptimVariables()
    {
        var costIntMrtx = new IntVar[NrStocks];
        for (var i = 0; i < NrStocks; i++)
            costIntMrtx[i] = _model.NewIntVar(0, int.MaxValue, $"N_{i}");

        return costIntMrtx;
    }

    public List<PortfolioValueDto> AllocateWithOnlySecurityConstraint(out CpSolverStatus resultStatus)
    {
        SetMoneyConstraint();

        SetSecurityConstraints();

        SetObjectiveFunc();

        // Solve
        resultStatus = solver.Solve(_model);


        return MakePortfolioValues();
    }
    
    public List<PortfolioValueDto> AllocateWithOnlySectorConstraint(out CpSolverStatus resultStatus)
    {
        SetMoneyConstraint();

        SetSecurityConstraints();
        SetSectorConstraints();
        
        SetObjectiveFunc();

        // Solve
        resultStatus = solver.Solve(_model);

        return MakePortfolioValues();
    }
    
    public List<PortfolioValueDto> AllocateWithSectorCountrySecuritiesConstraint(out CpSolverStatus resultStatus)
    {
        SetMoneyConstraint();

        SetSecurityConstraints();
        SetCountryConstraints();
        SetSectorConstraints();

        SetObjectiveFunc();

        // Solve
        resultStatus = solver.Solve(_model);

        return MakePortfolioValues();
    }

    private void SetCountryConstraints()
    {
        // var maxCountryMoney = (double)(_input.Money / _uniqueCountries.Count);
        var maxCountryMoney = (double)_input.MaxCountryMoney;

        // Add money constraints on Countries
        bool countryWeightsExist = _input.CountryWeight.Any();
        foreach (string country in _uniqueCountries)
        {
            if (countryWeightsExist && _input.CountryWeight.TryGetValue(country, out double weight))
                maxCountryMoney = (double)_input.Money * weight;
            else if (countryWeightsExist) // if weights exist but not for this sector -> continue
                maxCountryMoney = 0;

            var securitiesCountryIndexList = _securities
                .Select((item, index) => new { item, index })
                .Where(s => s.item.Security.Country == country)
                .Select(s => s.index)
                .ToHashSet();

            var countryVariables = new List<IntVar>();
            var prices = new List<int>();
            for (var i = 0; i < NrStocks; i++)
            {
                if (!securitiesCountryIndexList.Contains(i)) 
                    continue;
                
                double securityPrice = _securities[i].Price.MedianPrice();
                countryVariables.Add(X_NrStocks[i]);
                prices.Add((int)Math.Ceiling(securityPrice));
            }

            var maxCountryMoneyNoDecimal = (int)Math.Ceiling(maxCountryMoney);
            _model.Add(0 <= LinearExpr.WeightedSum(countryVariables, prices) <= maxCountryMoneyNoDecimal);
        }
    }

    private void SetSectorConstraints()
    {
        var maxSectorMoney = (double)(_input.Money / _uniqueSectors.Count);
        var minSectorMoney = (double)_input.Money / Math.Pow(_uniqueSectors.Count, 2);

        // Add money constraints on Sectors
        bool sectorWeightsExist = _input.SectorWeight.Any();
        foreach (string sector in _uniqueSectors)
        {
            if (sectorWeightsExist && _input.SectorWeight.TryGetValue(sector, out double weight))
                maxSectorMoney = (double)(_input.Money * (decimal)weight);
            else if (sectorWeightsExist) // if weights exist but not for this sector -> continue
                maxSectorMoney = 0;

            var securitiesSectorIndexList = _securities
                .Select((item, index) => new { item, index })
                .Where(s => s.item.Security.Sector == sector)
                .Select(s => s.index)
                .ToHashSet();
            
            var sectorVariables = new List<IntVar>();
            var prices = new List<int>();
            for (var i = 0; i < NrStocks; i++)
            {
                if (!securitiesSectorIndexList.Contains(i)) 
                    continue;

                double securityPrice = _securities[i].Price.MedianPrice();
                sectorVariables.Add(X_NrStocks[i]);
                prices.Add((int)Math.Ceiling(securityPrice));
            }

            var maxSectorMoneyNoDecimal = (int)Math.Ceiling(maxSectorMoney);
            var minSectorMoneyNoDecimal = (int)Math.Ceiling(minSectorMoney);
            _model.Add(minSectorMoneyNoDecimal <= LinearExpr.WeightedSum(sectorVariables, prices) <= maxSectorMoneyNoDecimal);
        }
    }

    /// <summary>
    /// </summary>
    private void SetSecurityConstraints()
    {
        // Take max price in securities here?
        for (var i = 0; i < NrStocks; i++)
        {
            //non-integer coefficients, you must first multiply the entire constraint by a sufficiently large integer to convert the coefficients to integers. In this case , you can multiply by 2, which results in the new constraint
            var priceNoDecimal = (int)Math.Ceiling(_securities[i].Price.MedianPrice());
            var maxSecuritySpendingNodecimal = (int)Math.Ceiling(_input.MaxSecuritySpending);
            _model.Add(0 <= priceNoDecimal * X_NrStocks[i] <= maxSecuritySpendingNodecimal);
        }
    }

    /// <summary>
    ///     Set constraint so that we do not spend more money than we have
    /// </summary>
    private void SetMoneyConstraint()
    {
        var prices = new List<int>();
        for (var j = 0; j < NrStocks; j++)
        {
            double securityPrice = _securities[j].Price.MedianPrice();
            prices.Add((int)Math.Ceiling(securityPrice));
        }

        var inputMoneyNoDecimal = (int)Math.Ceiling(_input.Money);
        _model.Add(0 <= LinearExpr.WeightedSum(X_NrStocks, prices) <= inputMoneyNoDecimal);
    }

    /// <summary>
    ///     Maximize: R_1*N_1 + R_2*N_2 + ... + R_i*N_i
    ///     Where
    ///     N = Amount of allocated securities to be bought of security i
    ///     R = Converted rank of security i (higher the better)
    /// </summary>
    private void SetObjectiveFunc()
    {
        var convRank = new List<int>();
        for (var i = 0; i < NrStocks; i++)
        {
            InternalSecurityRank security = _securities[i];
            convRank.Add(security.Rank.FunctionConvertedRank);
        }

        _model.Maximize(LinearExpr.WeightedSum(X_NrStocks, convRank));
    }

    private List<PortfolioValueDto> MakePortfolioValues()
    {
        var portfolioValues = new List<PortfolioValueDto>();
        for (var i = 0; i < NrStocks; i++)
        {
            InternalSecurityRank security = _securities[i];
            var sol2 = solver.Value(X_NrStocks[i]);

            portfolioValues.Add(new()
            {
                SecurityId = security.Security.SecurityId,
                Rank = security.Rank.Rank,
                Price = security.Price.MedianPrice(),
                Amount = (int)sol2
            });
        }

        // add the weights
        var totalPriceSum = portfolioValues.Sum(p => p.Price * p.Amount);
        foreach (var value in portfolioValues)
            value.Weight = value.Price * value.Amount / totalPriceSum;

        return portfolioValues;
    }
}