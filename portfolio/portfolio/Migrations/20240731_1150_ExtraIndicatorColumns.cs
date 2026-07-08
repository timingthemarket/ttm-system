using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240731_1150)]
public class ExtraIndicatorColumns : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Table("portfolio_indicators")
            .AddColumn("lookback_period").AsInt32().Nullable()
            .AddColumn("lookback_aggregator").AsInt32().Nullable();
    }
}