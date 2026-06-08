using TTM.Shared.Models.SecuritiesMasterdata;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Interfaces;

public interface IQrySecuritiesHandler
{
    Task<List<SecurityDto>> HandleGetSecurities(SecuritiesQry qry);
}