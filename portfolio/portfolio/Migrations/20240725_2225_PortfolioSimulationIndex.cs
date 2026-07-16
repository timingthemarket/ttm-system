using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240725_2225)]
public class PortfolioSimulationIndex : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Index("securities_date-IDX").OnTable("portfolio")
            .OnColumn("securities_date");

        Create.Index("percentage_change_init_money-IDX").OnTable("simulation")
            .OnColumn("percentage_change").Ascending()
            .OnColumn("init_money").Ascending();
        
    }
}