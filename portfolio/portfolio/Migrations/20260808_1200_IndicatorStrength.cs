using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20260808_1200)]
public class IndicatorStrengthTable : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("indicator_strength")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("indicator_id").AsInt32().NotNullable()
            .WithColumn("direction").AsInt32().NotNullable()
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("strength").AsDouble().NotNullable();

        // One strength value per indicator variant per rebalance date. The uniqueness is
        // relied upon by IndicatorStrengthRepository.SaveMany, which deletes the date first.
        Create.Index("ix_indicator_strength_indicator_id_direction_date")
            .OnTable("indicator_strength")
            .OnColumn("indicator_id").Ascending()
            .OnColumn("direction").Ascending()
            .OnColumn("date").Ascending()
            .WithOptions().Unique();
    }
}
