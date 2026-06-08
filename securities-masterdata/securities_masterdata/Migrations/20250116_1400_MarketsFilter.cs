using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20250116_1400)]
public class MarketsFilterColumn : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Table("markets")
            .AddColumn("inactive").AsBoolean().NotNullable().WithDefaultValue(false);
    }
}