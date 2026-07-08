using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20250303_2050)]
public class SessionTable : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("session")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("session_date").AsDate().NotNullable();

        Alter.Table("simulation")
            .AddColumn("session_id").AsInt32().Nullable().Indexed("ix_simulation_session_id");
        
        Create.ForeignKey("FK_simulation_sessions")
            .FromTable("simulation").ForeignColumn("session_id")
            .ToTable("session").PrimaryColumn("id");
    }
}