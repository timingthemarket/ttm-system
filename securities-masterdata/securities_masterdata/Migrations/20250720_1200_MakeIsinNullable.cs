using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20250720_1200)]
public class MakeIsinNullable : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Column("isin").OnTable("securities")
            .AsString(100).Nullable();
    }
}