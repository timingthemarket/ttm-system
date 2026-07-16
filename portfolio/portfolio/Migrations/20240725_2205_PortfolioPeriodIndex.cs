using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240725_2205)]
public class PortfolioPeriodIndex : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Index("simulation_id-IDX").OnTable("simulation_period")
            .OnColumn("simulation_id");
        
        Create.Index("portfolio_id-IDX").OnTable("simulation_period")
            .OnColumn("portfolio_id");
        
    }
}