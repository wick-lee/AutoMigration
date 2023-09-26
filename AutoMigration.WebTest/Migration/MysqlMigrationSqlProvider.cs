using Wick.AutoMigration.Config;
using Wick.AutoMigration.Interface;

namespace AutoMigration.WebTest.Migration;

public class MysqlMigrationSqlProvider : IMigrationSqlProvider<MysqlDbContext>
{
    public string CreateUpgradeHistoryTableScript()
    {
        return
            $"CREATE TABLE IF NOT EXISTS {RunTimeConfig.MysqlConfig.DataUpgradeTableName} ( serviceKey text not null, isRepeat bool not null, executedTime timestamp );";
    }

    public string CheckUpgradeHistoryTableExistScript()
    {
        return
            $"select count(1) from INFORMATION_SCHEMA.TABLES where TABLE_NAME='{RunTimeConfig.MysqlConfig.DataUpgradeTableName}';";
    }

    public string UpdateUpgradeHistoryTableSql()
    {
        return string.Empty;
    }

    public string CheckUpgradeServiceHasExecutedSql(IDataUpgradeService upgradeService)
    {
        return
            $"select count(1) from INFORMATION_SCHEMA.TABLES where TABLE_NAME='{RunTimeConfig.MysqlConfig.DataUpgradeTableName}';";
    }

    public string GetAddUpgradeRecordSql(IDataUpgradeService upgradeService)
    {
        return
            $"INSERT INTO {RunTimeConfig.MysqlConfig.DataUpgradeTableName} (serviceKey, isRepeat, executedTime) VALUES ('{upgradeService.Key}','{upgradeService.IsRepeat}','{DateTimeOffset.UtcNow}');";
    }

    public string CheckMigrationHistoryTableExistScript()
    {
        return
            $"select count(1) from INFORMATION_SCHEMA.TABLES where TABLE_NAME='{RunTimeConfig.MysqlConfig.MigrationTableName}';";
    }

    public string CreateMigrationHistoryTableScript()
    {
        return
            $"CREATE TABLE IF NOT EXISTS {RunTimeConfig.MysqlConfig.MigrationTableName} ( id int primary key auto_increment, runTime timestamp, metadata text not null, migrationId text not null, productVersion text not null, upOperations text not null, downOperations text not null, dbContextFullName text not null ,ingoretables text);";
    }

    public string UpdateMigrationHistoryTableSql()
    {
        return string.Empty;
    }
}