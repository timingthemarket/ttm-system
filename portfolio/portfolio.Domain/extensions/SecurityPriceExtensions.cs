using TTM.Shared.Models.SecuritiesMasterdata.Dto;

namespace portfolio.Domain.Extensions;

public static class SecurityPriceExtensions
{
    public static double MedianPrice(this SecurityPriceDto priceDto) =>
        (priceDto.High + priceDto.Low) / 2;
}