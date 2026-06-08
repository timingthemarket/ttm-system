using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20230507_2057)]
public class HistoricalRates_20230507_2057 : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("currency_rates")
            .WithColumn("currency_id_from").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("currency_id_to").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("date").AsDate().NotNullable().PrimaryKey()
            .WithColumn("rate").AsDouble().NotNullable();
        
        Create.ForeignKey("RASTES_HAS_CURRECYIDFROM").FromTable("currency_rates")
            .ForeignColumns("currency_id_from").ToTable("currencies").PrimaryColumns("currency_id");
        Create.ForeignKey("RASTES_HAS_CURRECYIDTO").FromTable("currency_rates")
            .ForeignColumns("currency_id_to").ToTable("currencies").PrimaryColumns("currency_id");
    }
}