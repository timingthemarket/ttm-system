using boersdata_raw.DataAccess.Constants;
using boersdata_raw.DataAccess.Models;
using boersdata_raw.DataAccess.Models.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace boersdata_raw.DataAccess;

public class BoersDataDbContext : DbContext
{
    public virtual DbSet<Security> Securities { get; set; } = null!;
    public virtual DbSet<StockPrice> StockPrices { get; set; } = null!;
    public virtual DbSet<Report> Reports { get; set; } = null!;
    public virtual DbSet<ReportTypes> ReportTypes { get; set; } = null!;
    public virtual DbSet<Country> Countries { get; set; } = null!;
    public virtual DbSet<Market> Markets { get; set; } = null!;
    public virtual DbSet<Sector> Sectors { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql(Configuration.DbConString, options => options.CommandTimeout(30));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Npgsql requires DateTimeKind.Utc for timestamptz columns; API-parsed dates can be Unspecified
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v == null ? null : v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc),
            v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        modelBuilder.Entity<Security>(entity =>
        {
            entity.ToTable("security");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(s => s.Origin).HasColumnName("origin").HasMaxLength(10)
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<SecurityOrigin>(v, true));
            entity.Property(s => s.Ticker).HasColumnName("ticker").HasMaxLength(30);
            entity.Property(s => s.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(s => s.Isin).HasColumnName("isin").HasMaxLength(20);
            entity.Property(s => s.Type).HasColumnName("type").HasMaxLength(20).HasConversion<string>();
            entity.Property(s => s.MarketId).HasColumnName("market_id");
            entity.Property(s => s.CountryId).HasColumnName("country_id");
            entity.Property(s => s.SectorId).HasColumnName("sector_id");
            entity.Property(s => s.IndustryId).HasColumnName("industry_id");
            entity.Property(s => s.YahooTicker).HasColumnName("yahoo_ticker").HasMaxLength(30);
            entity.Property(s => s.InsId).HasColumnName("ins_id");
            entity.Property(s => s.UrlName).HasColumnName("url_name").HasMaxLength(200);
            entity.Property(s => s.Currency).HasColumnName("currency").HasMaxLength(10);
            entity.Property(s => s.ReportCurrency).HasColumnName("report_currency").HasMaxLength(10);
        });

        modelBuilder.Entity<StockPrice>(entity =>
        {
            entity.ToTable("stock_price");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(p => p.InsId).HasColumnName("ins_id");
            entity.Property(p => p.Ticker).HasColumnName("ticker").HasMaxLength(30);
            entity.Property(p => p.Open).HasColumnName("open");
            entity.Property(p => p.Close).HasColumnName("close");
            entity.Property(p => p.High).HasColumnName("high");
            entity.Property(p => p.Low).HasColumnName("low");
            entity.Property(p => p.Volume).HasColumnName("volume");
            entity.Property(p => p.Date).HasColumnName("date");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("report");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(r => r.Ticker).HasColumnName("ticker").HasMaxLength(30);
            entity.Property(r => r.InsId).HasColumnName("ins_id");
            entity.Property(r => r.ReportType).HasColumnName("report_type").HasMaxLength(10).HasConversion<string>();
            entity.Property(r => r.Year).HasColumnName("year");
            entity.Property(r => r.Period).HasColumnName("period");

            entity.OwnsOne(r => r.CashFlow, cashFlow =>
            {
                cashFlow.Property(c => c.FreeCashFlow).HasColumnName("free_cash_flow");
                cashFlow.Property(c => c.OperatingActivities).HasColumnName("operating_activities");
                cashFlow.Property(c => c.InvestingActivities).HasColumnName("investing_activities");
                cashFlow.Property(c => c.FinancingActivities).HasColumnName("financing_activities");
                cashFlow.Property(c => c.CashFlowForTheYear).HasColumnName("cash_flow_for_the_year");
            });

            entity.OwnsOne(r => r.BalanceSheet, balanceSheet =>
            {
                balanceSheet.Property(b => b.GrossIncome).HasColumnName("gross_income");
                balanceSheet.Property(b => b.NetDebt).HasColumnName("net_debt");
                balanceSheet.Property(b => b.IntangibleAssets).HasColumnName("intangible_assets");
                balanceSheet.Property(b => b.TangibleAssets).HasColumnName("tangible_assets");
                balanceSheet.Property(b => b.CurrentAssets).HasColumnName("current_assets");
                balanceSheet.Property(b => b.NonCurrentAssets).HasColumnName("non_current_assets");
                balanceSheet.Property(b => b.TotalAssets).HasColumnName("total_assets");
                balanceSheet.Property(b => b.ProfitToEquityHolders).HasColumnName("profit_to_equity_holders");
                balanceSheet.Property(b => b.NonCurrentLiabilities).HasColumnName("non_current_liabilities");
                balanceSheet.Property(b => b.CurrentLiabilities).HasColumnName("current_liabilities");
                balanceSheet.Property(b => b.TotalLiabilitiesAndEquity).HasColumnName("total_liabilities_and_equity");
                balanceSheet.Property(b => b.CashAndEquivalents).HasColumnName("cash_and_equivalents");
                balanceSheet.Property(b => b.TotalEquity).HasColumnName("total_equity");
                balanceSheet.Property(b => b.FinancialAssets).HasColumnName("financial_assets");
            });

            entity.OwnsOne(r => r.IncomeStatement, incomeStatement =>
            {
                incomeStatement.Property(i => i.Eps).HasColumnName("eps");
                incomeStatement.Property(i => i.OperatingIncome).HasColumnName("operating_income");
                incomeStatement.Property(i => i.Revenues).HasColumnName("revenues");
                incomeStatement.Property(i => i.GrossProfit).HasColumnName("gross_profit");
                incomeStatement.Property(i => i.NetSales).HasColumnName("net_sales");
            });

            entity.OwnsOne(r => r.ReportKpis, kpis =>
            {
                kpis.Property(k => k.ROC).HasColumnName("roc");
                kpis.Property(k => k.ROIC).HasColumnName("roic");
                kpis.Property(k => k.FScore).HasColumnName("f_score");
            });

            entity.Property(r => r.NumberOfShares).HasColumnName("number_of_shares");
            entity.Property(r => r.Dividend).HasColumnName("dividend");
            entity.Property(r => r.StockPriceAverage).HasColumnName("stock_price_average");
            entity.Property(r => r.StockPriceHigh).HasColumnName("stock_price_high");
            entity.Property(r => r.StockPriceLow).HasColumnName("stock_price_low");
            entity.Property(r => r.ReportStartDate).HasColumnName("report_start_date").HasConversion(nullableUtcConverter);
            entity.Property(r => r.ReportEndDate).HasColumnName("report_end_date").HasConversion(nullableUtcConverter);
            entity.Property(r => r.ReportDate).HasColumnName("report_date").HasConversion(nullableUtcConverter);
            entity.Property(r => r.BrokenFiscalYear).HasColumnName("broken_fiscal_year");
            entity.Property(r => r.Currency).HasColumnName("currency").HasMaxLength(10);
            entity.Property(r => r.CurrencyRatio).HasColumnName("currency_ratio");
        });

        modelBuilder.Entity<ReportTypes>(entity =>
        {
            entity.ToTable("report_types");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(t => t.Name).HasColumnName("name").HasMaxLength(100);
            entity.OwnsOne(t => t.Translations, translations =>
            {
                translations.Property(x => x.NameSv).HasColumnName("name_sv").HasMaxLength(100);
                translations.Property(x => x.NameEn).HasColumnName("name_en").HasMaxLength(100);
            });
            entity.Property(t => t.ReportProperty).HasColumnName("report_property").HasMaxLength(100);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("country");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(c => c.Name).HasColumnName("name").HasMaxLength(100);
            entity.OwnsOne(c => c.Translations, translations =>
            {
                translations.Property(x => x.NameSv).HasColumnName("name_sv").HasMaxLength(100);
                translations.Property(x => x.NameEn).HasColumnName("name_en").HasMaxLength(100);
            });
            entity.Property(c => c.CountryId).HasColumnName("country_id");
        });

        modelBuilder.Entity<Market>(entity =>
        {
            entity.ToTable("market");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(m => m.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(m => m.MarketId).HasColumnName("market_id");
        });

        modelBuilder.Entity<Sector>(entity =>
        {
            entity.ToTable("sector");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(s => s.Name).HasColumnName("name").HasMaxLength(100);
            entity.OwnsOne(s => s.Translations, translations =>
            {
                translations.Property(x => x.NameSv).HasColumnName("name_sv").HasMaxLength(100);
                translations.Property(x => x.NameEn).HasColumnName("name_en").HasMaxLength(100);
            });
            entity.Property(s => s.SectorId).HasColumnName("sector_id");

            entity.OwnsMany(s => s.Industries, industry =>
            {
                industry.ToTable("industry");
                industry.WithOwner().HasForeignKey("sector_id");
                industry.Property<long>("id").ValueGeneratedOnAdd();
                industry.HasKey("id");
                industry.Property(i => i.Name).HasColumnName("name").HasMaxLength(100);
                industry.OwnsOne(i => i.Translations, translations =>
                {
                    translations.Property(x => x.NameSv).HasColumnName("name_sv").HasMaxLength(100);
                    translations.Property(x => x.NameEn).HasColumnName("name_en").HasMaxLength(100);
                });
                industry.Property(i => i.IndustryId).HasColumnName("industry_id");
            });
        });
    }
}
