using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240421_1130)]
public class ExplorerSimulation : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("explorer")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("completed").AsDateTime().Nullable()
            .WithColumn("registered").AsDateTime().NotNullable()
            .WithColumn("best_simulation_id").AsGuid().Nullable();
        
        Create.Table("explorer_simulation")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("explorer_id").AsGuid().NotNullable()
            .WithColumn("simulation_id").AsGuid().NotNullable();
        
        Create.ForeignKey("EXPLORER_SIMULATION_HAS_EXPLORER").FromTable("explorer_simulation")
            .ForeignColumn("explorer_id").ToTable("explorer").PrimaryColumn("id");
    }
}