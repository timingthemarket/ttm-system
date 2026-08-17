using FluentMigrator;

namespace article_news_raw.Migrations;

[Migration(20260817_1300)]
public class EconomicIndicators : ForwardOnlyMigration
{
    public override void Up()
    {
        // indicator_type holds "INFLATION" or "FEDERAL_FUNDS_RATE". The composite key on
        // (date, indicator_type) makes repeated fetches idempotent.
        Create.Table("economic_indicator")
            .WithColumn("date").AsDate().NotNullable().PrimaryKey()
            .WithColumn("indicator_type").AsString(50).NotNullable().PrimaryKey()
            .WithColumn("value").AsDouble().NotNullable();
    }
}
