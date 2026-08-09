using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20260809_1200)]
public class TradePlatformColumn : ForwardOnlyMigration
{
    public override void Up()
    {
        // Null means the security is not tradable on any platform we sync against, i.e. inactive.
        // Otherwise a comma separated list: "Avanza", "Nordnet" or "Avanza, Nordnet".
        Alter.Table("securities")
            .AddColumn("trade_platform").AsString(100).Nullable();
    }
}
