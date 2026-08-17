using System.Runtime.Serialization;

namespace TTM.Shared.Models.ArticleNewsRaw;

[DataContract]
public class EconomicIndicatorDto
{
    /// <summary>
    /// First day of the period the observation covers.
    /// </summary>
    [DataMember(Order = 1, IsRequired = true)]
    public DateOnly Date { get; set; }

    /// <summary>
    /// One of <see cref="Constants.EconomicIndicatorTypes"/>.
    /// </summary>
    [DataMember(Order = 2, IsRequired = true)]
    public string IndicatorType { get; set; }

    /// <summary>
    /// Observed value for <see cref="Date"/>, in percent.
    /// </summary>
    [DataMember(Order = 3, IsRequired = true)]
    public double Value { get; set; }
}
