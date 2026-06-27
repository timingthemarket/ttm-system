using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.BoersDataApi;

namespace boersdata_raw.DataAccess.Interfaces;
public interface IBoersDataService
{
    public Task<IReadOnlyList<BoersDataInstrument>> GetNordicInstruments();
    public Task<IReadOnlyList<BoersDataInstrument>> GetGlobalInstruments();
    public Task<IReadOnlyList<BoersDataIndustry>> GetIndustries();
    public Task<IReadOnlyList<BoersDataCountry>> GetCountries();
    public Task<IReadOnlyList<BoersDataMarket>> GetMarkets();
    public Task<IReadOnlyList<BoersDataSector>> GetSectors();
    public Task<IReadOnlyList<BoersDataTranslationMetadata>> GetTranslations();
    public Task<IReadOnlyList<BoersDataLatestStockPrice>> GetLatestNordicStockPrices();
    Task<IReadOnlyList<BoersDataLatestStockPrice>> GetLatestGlobalStockPrices();
    public Task<IReadOnlyList<BoersDataStockPriceArray>> GetHistoricalStockPrices(
        HashSet<long> instrumentIds);
    public Task<IReadOnlyList<BoersDataReportList>> GetReports(HashSet<long> instrumentIds);
    Task<InstrumentsKpiHistory?> GetR12KpiHistory(int kpiId, List<long> instrumentIds);
}
