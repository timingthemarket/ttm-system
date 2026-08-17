using FluentMigrator;

namespace article_news_raw.Migrations;

[Migration(20260817_1200)]
public class IndexData : ForwardOnlyMigration
{
    public override void Up()
    {
        // index_type holds "SPX" or "VIX". The composite key on
        // (date, index_type) makes repeated daily fetches idempotent.
        Create.Table("index_data")
            .WithColumn("date").AsDate().NotNullable().PrimaryKey()
            .WithColumn("index_type").AsString(50).NotNullable().PrimaryKey()
            .WithColumn("value").AsDouble().NotNullable();
    }
}
