using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240804_1150)]
public class SimulationBestArchive : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("simulation_best_archive")
            .WithColumn("simulation_id").AsGuid().NotNullable().Unique();
        
        Create.ForeignKey("SIMULATION_BEST_ARCHIVE_HAS_SIMULATION").FromTable("simulation_best_archive")
            .ForeignColumn("simulation_id").ToTable("simulation").PrimaryColumn("id");
    }
}