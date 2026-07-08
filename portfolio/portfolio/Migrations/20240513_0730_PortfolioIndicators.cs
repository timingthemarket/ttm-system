using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240513_0730)]
public class PortfolioIndicators : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("portfolio_indicators")
            .WithColumn("portfolio_id").AsGuid().PrimaryKey()
            .WithColumn("indicator").AsInt32().PrimaryKey()
            .WithColumn("weight").AsDouble().NotNullable()
            .WithColumn("direction").AsInt32().NotNullable()
            .WithColumn("lookback").AsString(100).NotNullable();

        Create.Index("IX_protfolio_id_indicators").OnTable("portfolio_indicators").OnColumn("portfolio_id");
        
        Create.ForeignKey("PORTFOLIO-INDICATORS_HAS_PORTFOLIO").FromTable("portfolio_indicators")
            .ForeignColumn("portfolio_id").ToTable("portfolio").PrimaryColumn("id");

        Create.Table("explorer_hash")
            .WithColumn("explorer_id").AsGuid().NotNullable()
            .WithColumn("attribute_hash").AsString(100).Nullable().Unique("ATTRIBUTE-HASH_UNIQUE");

        Create.ForeignKey("EXPLORER-HASH_HAS_EXPLORER").FromTable("explorer_hash")
            .ForeignColumn("explorer_id").ToTable("explorer").PrimaryColumn("id");
    }
}