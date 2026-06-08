using System.Runtime.Serialization;

namespace TTM.Shared.Models.BoersDataRaw.Securities;

[DataContract]
public class CountryDto
{
    [DataMember(Order = 1, IsRequired = true)]
    public string Name { get; set; }
}