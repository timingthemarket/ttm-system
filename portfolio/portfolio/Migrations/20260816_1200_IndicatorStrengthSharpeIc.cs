using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20260816_1200)]
public class IndicatorStrengthSharpeIcColumns : ForwardOnlyMigration
{
    public override void Up()
    {
        // The raw statistics replace the normalized strength value: unlike strength, which only
        // ranked an indicator against its peers at the same date, these are comparable across dates.
        Alter.Table("indicator_strength")
            .AddColumn("sharpe_ratio").AsDouble().Nullable()
            .AddColumn("ic").AsDouble().Nullable();

        // Both numbers were already carried in the metadata JSON, so existing rows can keep them.
        Execute.Sql(
            """
            UPDATE indicator_strength
            SET sharpe_ratio = (metadata::json ->> 'sharpe')::double precision,
                ic = (metadata::json ->> 'ic')::double precision
            WHERE metadata IS NOT NULL;
            """);

        // Rows written before the metadata column hold nothing but the strength value that is
        // dropped below, so there is no sharpe ratio to reconstruct for them.
        Execute.Sql("DELETE FROM indicator_strength WHERE sharpe_ratio IS NULL;");

        Alter.Column("sharpe_ratio").OnTable("indicator_strength").AsDouble().NotNullable();

        Delete.Column("strength").Column("metadata").FromTable("indicator_strength");
    }
}
