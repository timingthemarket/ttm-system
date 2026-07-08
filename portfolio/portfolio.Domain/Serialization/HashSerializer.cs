using System.Text.Json.Serialization;
using portfolio.Domain.Models;

namespace portfolio.Domain.Serialization;

[JsonSerializable(typeof(PortfolioInput))]
public partial class HashSerializer : JsonSerializerContext
{
    
}