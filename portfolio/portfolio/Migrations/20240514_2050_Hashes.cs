using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20240514_2050)]
public class ExplorerHash : ForwardOnlyMigration
{
    public override void Up()
    {
        Delete.Column("init_money").FromTable("simulation_period");

        Delete.Table("explorer_hash");

        Create.ForeignKey("EXPLORER-SIMULATION_HAS_SIMULATION").FromTable("explorer_simulation")
            .ForeignColumn("simulation_id").ToTable("simulation").PrimaryColumn("id");

        Alter.Table("explorer_simulation").AddColumn("attribute_hash").AsString(100).Unique();
    }
}