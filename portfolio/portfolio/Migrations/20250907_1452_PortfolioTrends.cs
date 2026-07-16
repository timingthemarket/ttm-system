using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20250907_1452)]
public class PortfolioTrends : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("portfolio_trends")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("set_id").AsString(32).NotNullable()
            .WithColumn("revision").AsInt32().NotNullable()
            .WithColumn("beta0").AsDouble().Nullable()
            .WithColumn("beta1").AsDouble().Nullable()
            .WithColumn("beta2").AsDouble().Nullable()
            .WithColumn("timestamp").AsDateTime().NotNullable();

        Create.Index("ix_portfolio_trends_set_id").OnTable("portfolio_trends")
            .OnColumn("set_id");
    }
}