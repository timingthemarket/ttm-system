using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20230811_1255)]
public class indicators_20230811_1255 : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("indicators")
            .WithColumn("indicator_id").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("security_id").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("date").AsDate().NotNullable().PrimaryKey()
            .WithColumn("value").AsDecimal().NotNullable();
        
        Create.ForeignKey("indicators_HAS_security_id").FromTable("indicators")
            .ForeignColumns("security_id").ToTable("securities").PrimaryColumns("security_id");
    }
}