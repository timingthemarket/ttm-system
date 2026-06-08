using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20230507_1050)]
public class Securities_20230507_1050 : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("securities")
            .WithColumn("security_id").AsInt64().Identity().PrimaryKey()
            .WithColumn("ticker").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(600).NotNullable()
            .WithColumn("isin").AsString(100).NotNullable()
            .WithColumn("market_id").AsInt64().NotNullable()
            .WithColumn("currency_id").AsInt64().NotNullable()
            .WithColumn("industry").AsString(Int32.MaxValue).Nullable()
            .WithColumn("sector").AsString(Int32.MaxValue).NotNullable()
            .WithColumn("country").AsString(Int32.MaxValue).NotNullable()
            .WithColumn("description").AsString(Int32.MaxValue).Nullable()
            .WithColumn("updated").AsDateTime().NotNullable();

        Create.Table("markets")
            .WithColumn("market_id").AsInt64().Identity().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("open_time").AsTime().Nullable()
            .WithColumn("close_time").AsTime().Nullable()
            .WithColumn("updated").AsDateTime().NotNullable();

        Create.Table("currencies")
            .WithColumn("currency_id").AsInt64().Identity().PrimaryKey()
            .WithColumn("currency_code").AsString(3).NotNullable()
            .WithColumn("name").AsString(Int32.MaxValue).NotNullable()
            .WithColumn("updated").AsDateTime().NotNullable();
        
        
        Create.ForeignKey("SECURITY_HAS_MARKET").FromTable("securities")
            .ForeignColumn("market_id").ToTable("markets").PrimaryColumn("market_id");
        
        Create.ForeignKey("SECURITY_HAS_CURRENCY").FromTable("securities")
            .ForeignColumn("currency_id").ToTable("currencies").PrimaryColumn("currency_id");
    }
}