using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20241008_2100)]
public class YahooColumn : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Table("securities")
            .AddColumn("yahoo_ticker").AsString(100).Nullable()
            .AddColumn("inactive").AsBoolean().NotNullable().WithDefaultValue(false);
    }
}