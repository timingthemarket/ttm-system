using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240616_1250)]
public class PortfolioHash : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Table("portfolio").AddColumn("hash").AsString(100).Unique();

        Delete.Column("attribute_hash").FromTable("explorer_simulation");

    }
}