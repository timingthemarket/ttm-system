using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace boersdata_raw.DataAccess.Models.Report;

public enum ReportType
{
    Year,
    Quarter,
    TTM
}

public record Report
{
    [BsonIgnoreIfDefault] public ObjectId Id { get; set; }

    public string Ticker { get; set; } = null!;
    public long InsId { get; set; }

    [BsonRepresentation(BsonType.String)] public ReportType ReportType { get; set; }

    public int Year { get; set; }
    public int Period { get; set; }

    public ReportCashFlow CashFlow { get; init; } = new();
    public ReportBalanceSheet BalanceSheet { get; init; } = new();
    public ReportIncomeStatement IncomeStatement { get; init; } = new();
    public ReportKpis ReportKpis { get; set; } = new();
    public double NumberOfShares { get; set; }
    public double Dividend { get; set; }
    public double StockPriceAverage { get; set; }
    public double StockPriceHigh { get; set; }
    public double StockPriceLow { get; set; }
    public DateTime? ReportStartDate { get; set; }
    public DateTime? ReportEndDate { get; set; }
    public bool BrokenFiscalYear { get; set; }
    public string? Currency { get; set; }
    public double CurrencyRatio { get; set; }
    public DateTime? ReportDate { get; set; }
}