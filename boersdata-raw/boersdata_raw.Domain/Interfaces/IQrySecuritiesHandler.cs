using TTM.Shared.Models.BoersDataRaw.Securities;

namespace boersdata_raw.Domain.Interfaces;

public interface IQrySecuritiesHandler
{
    Task<List<SecurityDto>> HandleGetSecurities();
}