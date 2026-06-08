using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.DataAccess.Services.Models;
using securities_masterdata.Domain.Interfaces;
using securities_masterdata.Domain.Models.Sync;

namespace securities_masterdata.Domain.Handlers.Sync;

public class AvanzaSyncHandler : IAvanzaSyncHandler
{
    private readonly IAvanzaService _avanzaService;
    private readonly ISecurityRepository _securityRepository;

    public AvanzaSyncHandler(IAvanzaService avanzaService, ISecurityRepository securityRepository)
    {
        _avanzaService = avanzaService;
        _securityRepository = securityRepository;
    }

    public async Task<AvanzaSyncResult> HandleSyncSecuritiesWithAvanza(CancellationToken cancellationToken = default)
    {
        var result = new AvanzaSyncResult();

        try
        {
            // Get all Avanza stocks with pagination
            var avanzaStocks = await GetAllAvanzaStocks(cancellationToken);
            result.TotalAvanzaStocks = avanzaStocks.Count;

            // Get all securities from database including inactive ones
            var dbSecurities = await _securityRepository.GetAll(includeInactive: true);
            result.TotalSecuritiesInDatabase = dbSecurities.Count;

            // Create HashSet of Avanza tickers for efficient lookup
            var avanzaTickers = avanzaStocks
                .Where(stock => !string.IsNullOrWhiteSpace(stock.Ticker))
                .Select(stock => stock.Ticker)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Create dictionary of Avanza names mapped to tickers for name-based fallback
            var avanzaNameToTicker = avanzaStocks
                .Where(stock => !string.IsNullOrWhiteSpace(stock.Name))
                .Select(s => s.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Find securities that exist in DB but not in Avanza response
            var securitiesToMarkInactive = new List<long>();
            
            foreach (var security in dbSecurities.Where(s => !s.Inactive))
            {
                // First check by ticker
                if (avanzaTickers.Contains(security.Ticker))
                    continue;

                // Fallback: check by name similarity
                if (!string.IsNullOrWhiteSpace(security.Name) && 
                    HasSimilarName(security.Name, avanzaNameToTicker))
                    continue;

                securitiesToMarkInactive.Add(security.SecurityId);
            }

            // Update inactive status
            if (securitiesToMarkInactive.Any())
            {
                await _securityRepository.UpdateInactiveStatus(securitiesToMarkInactive, inactive: true);
                result.SecuritiesMarkedInactive = securitiesToMarkInactive.Count;
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task<List<AvanzaStock>> GetAllAvanzaStocks(CancellationToken cancellationToken)
    {
        var allStocks = new List<AvanzaStock>();
        const int pageSize = 1000;
        var offset = 0;
        
        while (true)
        {
            var response = await _avanzaService.GetStocksAsync(
                offset: offset, 
                limit: pageSize, 
                cancellationToken: cancellationToken);

            if (response?.Stocks == null || !response.Stocks.Any())
                break;

            allStocks.AddRange(response.Stocks);

            // If we got fewer results than the page size, we've reached the end
            if (response.Stocks.Length < pageSize)
                break;

            offset += pageSize;
        }

        return allStocks;
    }

    private static bool HasSimilarName(string securityName, HashSet<string> avanzaNames)
    {
        const double similarityThreshold = 0.8;
        
        return avanzaNames.Any(avanzaName => 
            CalculateSimilarity(securityName, avanzaName) >= similarityThreshold);
    }

    private static double CalculateSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            return 0.0;

        if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        var sourceClean = CleanName(source);
        var targetClean = CleanName(target);

        if (string.Equals(sourceClean, targetClean, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        var distance = LevenshteinDistance(sourceClean.ToLowerInvariant(), targetClean.ToLowerInvariant());
        var maxLength = Math.Max(sourceClean.Length, targetClean.Length);
        
        return 1.0 - (double)distance / maxLength;
    }

    private static string CleanName(string cleanName)
    {
        var companySuffixes = new List<string>
        {
            // Limited Liability Companies
            "Ltd",
            "LLC",
            "GmbH",
            "Sàrl",
            "BV",
            "ApS",
            "AS",
            "AB",
            "Oy",

            // Public Limited Companies
            "PLC",
            "AG",
            "SA",
            "SpA",
            "NV",
            "A/S",

            // US Corporations
            "Inc.",
            "Inc",
            "Corp.",
            "Corp",
            "Co.",
            "Co",

            // Partnerships
            "LLP",
            "LP",

            // Other Structures
            "SE",
            "SCE",
            "REIT",
            "SICAV"
        };

        foreach (string suffix in companySuffixes)
        {
            if (cleanName.EndsWith(" " + suffix, StringComparison.OrdinalIgnoreCase))
            {
                cleanName = cleanName.Substring(0, cleanName.Length - suffix.Length - 1).Trim();
                break; // Stop after first match
            }
            else if (cleanName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                cleanName = cleanName.Substring(0, cleanName.Length - suffix.Length).Trim();
                break;
            }
        }
        
        return cleanName
            .Trim();
    }

    private static int LevenshteinDistance(string source, string target)
    {
        if (source.Length == 0) return target.Length;
        if (target.Length == 0) return source.Length;

        var matrix = new int[source.Length + 1, target.Length + 1];

        for (var i = 0; i <= source.Length; i++)
            matrix[i, 0] = i;

        for (var j = 0; j <= target.Length; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }
}