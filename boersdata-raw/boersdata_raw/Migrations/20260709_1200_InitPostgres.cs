using FluentMigrator;

namespace boersdata_raw.Migrations;

[Migration(20260709_1200)]
public class InitPostgres : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("security")
            .WithColumn("id").AsInt64().Identity().PrimaryKey()
            .WithColumn("origin").AsString(10).NotNullable() // 'nordic' | 'global'
            .WithColumn("ticker").AsString(30).NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("isin").AsString(20).NotNullable()
            .WithColumn("type").AsString(20).NotNullable()
            .WithColumn("market_id").AsInt64().NotNullable()
            .WithColumn("country_id").AsInt64().NotNullable()
            .WithColumn("sector_id").AsInt64().Nullable()
            .WithColumn("industry_id").AsInt64().Nullable()
            .WithColumn("yahoo_ticker").AsString(30).NotNullable()
            .WithColumn("ins_id").AsInt64().NotNullable()
            .WithColumn("url_name").AsString(200).NotNullable()
            .WithColumn("currency").AsString(10).Nullable()
            .WithColumn("report_currency").AsString(10).Nullable();

        // Tickers are only unique within an origin (nordic vs global collections in Mongo)
        Create.Index("ix_security_origin_ticker").OnTable("security")
            .OnColumn("origin").Ascending()
            .OnColumn("ticker").Ascending()
            .WithOptions().Unique();

        Create.Index("ix_security_ins_id").OnTable("security")
            .OnColumn("ins_id");

        Create.Table("stock_price")
            .WithColumn("id").AsInt64().Identity().PrimaryKey()
            .WithColumn("ins_id").AsInt64().NotNullable()
            .WithColumn("ticker").AsString(30).NotNullable()
            .WithColumn("open").AsDouble().Nullable()
            .WithColumn("close").AsDouble().Nullable()
            .WithColumn("high").AsDouble().Nullable()
            .WithColumn("low").AsDouble().Nullable()
            .WithColumn("volume").AsInt64().Nullable()
            .WithColumn("date").AsCustom("DATE").NotNullable();

        // Dedupe target for daily batch inserts; prefix serves reads by ticker and ticker+date range
        Create.Index("ix_stock_price_ticker_date").OnTable("stock_price")
            .OnColumn("ticker").Ascending()
            .OnColumn("date").Ascending()
            .WithOptions().Unique();

        // Upsert key for single-price saves
        Create.Index("ix_stock_price_ins_id_date").OnTable("stock_price")
            .OnColumn("ins_id").Ascending()
            .OnColumn("date").Ascending()
            .WithOptions().Unique();

        Create.Table("report")
            .WithColumn("id").AsInt64().Identity().PrimaryKey()
            .WithColumn("ticker").AsString(30).NotNullable()
            .WithColumn("ins_id").AsInt64().NotNullable()
            .WithColumn("report_type").AsString(10).NotNullable() // Year | Quarter | TTM
            .WithColumn("year").AsInt32().NotNullable()
            .WithColumn("period").AsInt32().NotNullable()
            .WithColumn("free_cash_flow").AsDouble().Nullable()
            .WithColumn("operating_activities").AsDouble().Nullable()
            .WithColumn("investing_activities").AsDouble().Nullable()
            .WithColumn("financing_activities").AsDouble().Nullable()
            .WithColumn("cash_flow_for_the_year").AsDouble().Nullable()
            .WithColumn("gross_income").AsDouble().Nullable()
            .WithColumn("net_debt").AsDouble().Nullable()
            .WithColumn("intangible_assets").AsDouble().Nullable()
            .WithColumn("tangible_assets").AsDouble().Nullable()
            .WithColumn("current_assets").AsDouble().Nullable()
            .WithColumn("non_current_assets").AsDouble().Nullable()
            .WithColumn("total_assets").AsDouble().Nullable()
            .WithColumn("profit_to_equity_holders").AsDouble().Nullable()
            .WithColumn("non_current_liabilities").AsDouble().Nullable()
            .WithColumn("current_liabilities").AsDouble().Nullable()
            .WithColumn("total_liabilities_and_equity").AsDouble().Nullable()
            .WithColumn("cash_and_equivalents").AsDouble().Nullable()
            .WithColumn("total_equity").AsDouble().Nullable()
            .WithColumn("financial_assets").AsDouble().Nullable()
            .WithColumn("eps").AsDouble().Nullable()
            .WithColumn("operating_income").AsDouble().Nullable()
            .WithColumn("revenues").AsDouble().Nullable()
            .WithColumn("gross_profit").AsDouble().Nullable()
            .WithColumn("net_sales").AsDouble().Nullable()
            .WithColumn("roc").AsDouble().Nullable()
            .WithColumn("roic").AsDouble().Nullable()
            .WithColumn("f_score").AsDouble().Nullable()
            .WithColumn("number_of_shares").AsDouble().NotNullable()
            .WithColumn("dividend").AsDouble().NotNullable()
            .WithColumn("stock_price_average").AsDouble().NotNullable()
            .WithColumn("stock_price_high").AsDouble().NotNullable()
            .WithColumn("stock_price_low").AsDouble().NotNullable()
            .WithColumn("report_start_date").AsCustom("timestamptz").Nullable()
            .WithColumn("report_end_date").AsCustom("timestamptz").Nullable()
            .WithColumn("report_date").AsCustom("timestamptz").Nullable()
            .WithColumn("broken_fiscal_year").AsBoolean().NotNullable()
            .WithColumn("currency").AsString(10).Nullable()
            .WithColumn("currency_ratio").AsDouble().NotNullable();

        // Serves GetReports(ticker, type); prefix serves delete-by-ticker on re-sync
        Create.Index("ix_report_ticker_report_type").OnTable("report")
            .OnColumn("ticker").Ascending()
            .OnColumn("report_type").Ascending();

        Create.Table("report_types")
            .WithColumn("id").AsInt64().Identity().PrimaryKey()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("name_sv").AsString(100).Nullable()
            .WithColumn("name_en").AsString(100).Nullable()
            .WithColumn("report_property").AsString(100).NotNullable();

        Create.Table("country")
            .WithColumn("id").AsInt64().Identity().PrimaryKey()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("name_sv").AsString(100).Nullable()
            .WithColumn("name_en").AsString(100).Nullable()
            .WithColumn("country_id").AsInt64().NotNullable();

        Create.Index("ix_country_name").OnTable("country")
            .OnColumn("name").Ascending()
            .WithOptions().Unique();

        Create.Table("market")
            .WithColumn("id").AsInt64().Identity().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("market_id").AsInt64().NotNullable();

        Create.Index("ix_market_name").OnTable("market")
            .OnColumn("name").Ascending()
            .WithOptions().Unique();

        Create.Table("sector")
            .WithColumn("id").AsInt64().Identity().PrimaryKey()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("name_sv").AsString(100).Nullable()
            .WithColumn("name_en").AsString(100).Nullable()
            .WithColumn("sector_id").AsInt64().NotNullable();

        Create.Index("ix_sector_name").OnTable("sector")
            .OnColumn("name").Ascending()
            .WithOptions().Unique();

        Create.Table("industry")
            .WithColumn("id").AsInt64().Identity().PrimaryKey()
            .WithColumn("sector_id").AsInt64().NotNullable()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("name_sv").AsString(100).Nullable()
            .WithColumn("name_en").AsString(100).Nullable()
            .WithColumn("industry_id").AsInt64().NotNullable();

        Create.Index("ix_industry_sector_id").OnTable("industry")
            .OnColumn("sector_id");

        Create.ForeignKey("fk_industry_sector").FromTable("industry")
            .ForeignColumn("sector_id").ToTable("sector").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);
    }
}
