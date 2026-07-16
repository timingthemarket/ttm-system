using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240616_1850)]
public class PortfolioRowSimilarity : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Table("portfolio").AddColumn("row_similarity").AsDouble().Nullable();
    }
}