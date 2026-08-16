using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20260811_1200)]
public class IndicatorStrengthMetadataColumn : ForwardOnlyMigration
{
    public override void Up()
    {
        // JSON with the raw Sharpe ratio and mean Information Coefficient the strength was
        // computed from. Nullable because rows written before this migration have none.
        Alter.Table("indicator_strength")
            .AddColumn("metadata").AsString(int.MaxValue).Nullable();
    }
}
