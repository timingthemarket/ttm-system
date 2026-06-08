using FluentMigrator;

namespace securities_masterdata.Migrations;

[Migration(20230507_1447)]
public class CurrenciesInsert_20230507_1447 : ForwardOnlyMigration
{
    public override void Up()
    {
        Execute.Sql(@"
            INSERT INTO currencies (currency_code, name, updated) VALUES 
                                   ('SEK', 'Swedish krone', NOW()),
                                   ('USD', 'Us dollar', NOW()),
                                   ('EUR', 'Euro', NOW()),
                                   ('NOK', 'Norwegian krone', NOW()),   
                                   ('GBP', 'Pound sterling', NOW()),
                                   ('DKK', 'Danish krone', NOW());

        ");
    }
}