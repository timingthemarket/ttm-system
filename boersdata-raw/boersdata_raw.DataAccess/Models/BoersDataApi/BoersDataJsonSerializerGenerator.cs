using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

[JsonSerializable(typeof(BoersDataLatestStockPrices))]
[JsonSerializable(typeof(BoersDataStockPrices))]
[JsonSerializable(typeof(BoersDataReports))]
public partial class BoersDataJsonSerializerGenerator : JsonSerializerContext
{

}
