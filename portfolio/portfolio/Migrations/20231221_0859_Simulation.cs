using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20231221_0859)]
public class Simulation : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("simulation")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("completed").AsDateTime().Nullable()
            .WithColumn("registered").AsDateTime().NotNullable()
            .WithColumn("percentage_change").AsDouble().Nullable()
            .WithColumn("init_money").AsDecimal().NotNullable();

        Create.Table("simulation_period")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("simulation_id").AsGuid().NotNullable()
            .WithColumn("portfolio_id").AsGuid().NotNullable()
            .WithColumn("init_money").AsDecimal().NotNullable()
            .WithColumn("invested_money").AsDecimal().NotNullable()
            .WithColumn("liquid_money").AsDecimal().NotNullable();

        Create.ForeignKey("SIMULATION-PERIOD_HAS_SIMULATION").FromTable("simulation_period")
            .ForeignColumn("simulation_id").ToTable("simulation").PrimaryColumn("id");
        Create.ForeignKey("SIMULATION-PERIOD_HAS_PORTFOLIO").FromTable("simulation_period")
            .ForeignColumn("portfolio_id").ToTable("portfolio").PrimaryColumn("id");
    }
}