using riksbanken_raw.DataAccess.Models;

namespace riksbanken_raw.DataAccess.Interfaces;

public interface IRiksbankenService
{
    Task<IReadOnlyList<RiksbankenObservation>> GetHistoricalObservations(string seriesId);
    Task<RiksbankenObservation> GetLatestObservation(string seriesId);
}