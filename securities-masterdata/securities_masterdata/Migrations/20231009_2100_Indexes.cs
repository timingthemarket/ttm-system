using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20231009_2100)]
public class indexes : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("indexes")
            .WithColumn("index_id").AsInt64().Identity().NotNullable().PrimaryKey()
            .WithColumn("name").AsString(1000).NotNullable()
            .WithColumn("base_date").AsDate().NotNullable();

        Create.Table("index_securities")
            .WithColumn("index_id").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("security_id").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("weight").AsDouble().NotNullable();

        Create.Table("index_values")
            .WithColumn("index_id").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("date").AsDate().NotNullable().PrimaryKey()
            .WithColumn("value").AsDecimal().NotNullable();

        
        Create.ForeignKey("INDEXVALUES_HAS_INDEX").FromTable("index_values")
            .ForeignColumn("index_id").ToTable("indexes").PrimaryColumn("index_id");

        Create.ForeignKey("INDEXSECURITIES_HAS_INDEX").FromTable("index_securities")
            .ForeignColumn("index_id").ToTable("indexes").PrimaryColumn("index_id");

        Create.ForeignKey("INDEXSECURITIES_HAS_SECURITY").FromTable("index_securities")
            .ForeignColumn("security_id").ToTable("securities").PrimaryColumn("security_id");

        Insert.IntoTable("indexes").Row(new { name = "Omx30Stockholm", base_date = "1986-09-30"});
    }
}