using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20231217_0850)]
public class Portfolio : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("portfolio")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("securities_date").AsDate().NotNullable()
            .WithColumn("calculation_datetime").AsDateTime().NotNullable()
            .WithColumn("strategy").AsInt32().NotNullable();
        
        Create.Table("portfolio_value")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("portfolio_id").AsGuid().PrimaryKey()
            .WithColumn("security_id").AsInt64().NotNullable()
            .WithColumn("weight").AsDouble().NotNullable()
            .WithColumn("rank").AsInt64().NotNullable()
            .WithColumn("amount").AsInt32().NotNullable()
            .WithColumn("price").AsDouble().NotNullable();

        Create.Index("IX_portfolio_id").OnTable("portfolio_value").OnColumn("portfolio_id");
        
        Create.ForeignKey("PORTFOLIO-VALUE_HAS_PORTFOLIO").FromTable("portfolio_value")
            .ForeignColumn("portfolio_id").ToTable("portfolio").PrimaryColumn("id");
    }
}