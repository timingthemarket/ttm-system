using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20250226_2050)]
public class RemovalOfUnusedTables : ForwardOnlyMigration
{
    public override void Up()
    {
        Delete.Table("simulation_best_archive");
        Delete.Table("explorer_simulation");
        Delete.Table("explorer");
    }
}