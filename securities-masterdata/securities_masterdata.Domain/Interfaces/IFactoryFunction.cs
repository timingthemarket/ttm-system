using securities_masterdata.DataAccess.Entities;
using TTM.Shared.Constants;
using TTM.Shared.Models;
using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace securities_masterdata.Domain.Interfaces;

public interface IFactoryFunction
{
    public Indicators Indicator { get; }

    /// <summary>
    /// </summary>
    /// <param name="securityIds">The security ids to for the functions to return values on</param>
    /// <param name="date">Date to base the calculation on</param>
    /// <returns></returns>
    Task<List<SecurityIndicatorDto>> Process(List<Security> securities, DateOnly date, LookBackPeriod lookBackPeriod);
}