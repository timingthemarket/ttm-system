namespace boersdata_raw.DataAccess.Models.YahooApi;

public class SparkV7Meta
{
    public string Currency { get; set; }
    public string Symbol { get; set; }
    public string ExchangeName { get; set; }
    public string InstrumentType { get; set; }
    public int? FirstTradeDate { get; set; }
    public int? RegularMarketTime { get; set; }
    public int? Gmtoffset { get; set; }
    public string Timezone { get; set; }
    public string ExchangeTimezoneName { get; set; }
    public double? RegularMarketPrice { get; set; }
    public double? ChartPreviousClose { get; set; }
    public double? PreviousClose { get; set; }
    public int? Scale { get; set; }
    public int? PriceHint { get; set; }
    public string DataGranularity { get; set; }
    public string Range { get; set; }
    public List<string> ValidRanges { get; set; }
}

public class SparkV7Response
{
    public SparkV7Meta Meta { get; set; }
    public List<int?> Timestamp { get; set; }
}

public class SparkV7Result
{
    public string Symbol { get; set; }
    public List<SparkV7Response> Response { get; set; }
}

public class SparkV7Spark
{
    public List<SparkV7Result> Result { get; set; }
    public object Error { get; set; }
}

public class SparkV7
{
    public SparkV7Spark Spark { get; set; }
}