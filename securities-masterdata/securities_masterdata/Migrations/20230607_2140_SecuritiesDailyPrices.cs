using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20230607_2140)]
public class SecuritiesDailyPrices_20230607_2140 : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("securities_prices")
            .WithColumn("security_id").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("date").AsDate().NotNullable().PrimaryKey()
            .WithColumn("open").AsDouble().NotNullable()
            .WithColumn("high").AsDouble().NotNullable()
            .WithColumn("low").AsDouble().NotNullable()
            .WithColumn("close").AsDouble().NotNullable()
            .WithColumn("volume").AsInt64().NotNullable();

        Create.ForeignKey("PRICES_HAS_SECURITIES").FromTable("securities_prices")
            .ForeignColumns("security_id").ToTable("securities").PrimaryColumns("security_id");
    }
}