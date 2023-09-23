using Wick.AutoMigration;
using Wick.AutoMigration.Config;
using Wick.AutoMigration.Interface;

namespace AutoMigration.WebTest.Migration;

public class NpgsqlMigrationSqlProvider : IMigrationSqlProvider<MyDbContext>
{
    public string CreateUpgradeHistoryTableScript()
    {
        return
            $"CREATE TABLE IF NOT EXISTS {AutoMigrationConfig.DataUpgradeTableName}(serviceKey text not null , isRepeat bool not null ,executedTime timestamp with time zone)";
    }

    public string CheckUpgradeHistoryTableExistScript()
    {
        return $"SELECT count(1) FROM pg_class WHERE relname = '{AutoMigrationConfig.DataUpgradeTableName}'";
    }

    public string UpdateUpgradeHistoryTableSql()
    {
        return string.Empty;
    }

    public string CheckUpgradeServiceHasExecutedSql(IDataUpgradeService upgradeService)
    {
        return $"SELECT count(1) FROM tableName WHERE \"serviceKey\" = '{upgradeService.Key}'";
    }

    public string GetAddUpgradeRecordSql(IDataUpgradeService upgradeService)
    {
        return
            $"INSERT INTO \"{AutoMigrationConfig.DataUpgradeTableName}\" (serviceKey, isRepeat, executedTime) VALUES ('{upgradeService.Key}','{upgradeService.IsRepeat}','{DateTimeOffset.UtcNow}');";
    }

    public string CheckMigrationHistoryTableExistScript()
    {
        return $"SELECT count(1) FROM pg_class WHERE relname = '{AutoMigrationConfig.MigrationTableName}'";
    }

    public string CreateMigrationHistoryTableScript()
    {
        return
            $"CREATE TABLE IF NOT EXISTS \"{AutoMigrationConfig.MigrationTableName}\"(id uuid primary key not null ,runTime timestamp with time zone not null ,metadata text not null ,migrationId text not null ,productVersion text not null ,upOperations text not null ,downOperations text not null ,dbContextFullName text not null);";
    }

    public string UpdateMigrationHistoryTableSql()
    {
        return $"ALTER TABLE {AutoMigrationConfig.MigrationTableName} ADD IF NOT EXISTS ingoretables text;";
    }
}