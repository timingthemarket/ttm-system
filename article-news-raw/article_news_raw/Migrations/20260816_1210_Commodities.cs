using FluentMigrator;

namespace article_news_raw.Migrations;

[Migration(20260816_1210)]
public class Commodities : ForwardOnlyMigration
{
    public override void Up()
    {
        // commodity_type holds "GOLD", "SILVER" or "BRENT". The composite key on
        // (date, commodity_type) makes repeated monthly fetches idempotent.
        Create.Table("commodities")
            .WithColumn("date").AsDate().NotNullable().PrimaryKey()
            .WithColumn("commodity_type").AsString(50).NotNullable().PrimaryKey()
            .WithColumn("value").AsDouble().NotNullable();
    }
}
