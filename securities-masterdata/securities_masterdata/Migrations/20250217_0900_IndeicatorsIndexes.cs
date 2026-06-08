using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20250217_0900)]
public class IndeicatorsIndexes : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Index("ix_indicators_date").OnTable("indicators")
            .OnColumn("date").Ascending();

        Create.Index("ix_indicators_security_id").OnTable("indicators")
            .OnColumn("security_id");

        Create.Index("ix_prices_date").OnTable("securities_prices")
            .OnColumn("date").Ascending();
    }
}