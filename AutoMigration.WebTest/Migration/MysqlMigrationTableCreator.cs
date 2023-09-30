using AutoMigration.WebTest.Extensions;
using Wick.AutoMigration.Interface;

namespace AutoMigration.WebTest.Migration;

public class MysqlMigrationTableCreator : IMigrationTableCreator<MysqlDbContext>
{
    private readonly ILogger<MysqlMigrationTableCreator> _logger;

    public MysqlMigrationTableCreator(ILogger<MysqlMigrationTableCreator> logger)
    {
        _logger = logger;
    }

    public Task CreateUpgradeTableIfNotExist(MysqlDbContext dbContext)
    {
        var result = dbContext.Database.ExecuteScalar(
            $"CREATE TABLE IF NOT EXISTS {RunTimeConfig.MysqlConfig.DataUpgradeTableName} ( serviceKey text not null, isRepeat bool not null, executedTime timestamp );");
        if (result == null || (long)result == 0)
        {
            _logger.LogWarning("Create upgrade table failed");
        }

        return Task.CompletedTask;
    }

    public Task UpdateUpgradeTable(MysqlDbContext dbContext)
    {
        return Task.CompletedTask;
    }

    public Task CreateMigrationTableIfNotExist(MysqlDbContext dbContext)
    {
        var result = dbContext.Database.ExecuteScalar(
            $"CREATE TABLE IF NOT EXISTS {RunTimeConfig.MysqlConfig.MigrationTableName} ( id int primary key auto_increment, runTime timestamp, metadata text not null, migrationId text not null, productVersion text not null, upOperations text not null, downOperations text not null, dbContextFullName text not null ,ingoretables text);");

        if (result == null || (long)result == 0)
        {
            _logger.LogWarning("Create migration history table failed");
        }

        return Task.CompletedTask;
    }

    public Task UpdateMigrationTable(MysqlDbContext dbContext)
    {
        return Task.CompletedTask;
    }
}