using AutoMigration.WebTest.Extensions;
using Wick.AutoMigration.Interface;

namespace AutoMigration.WebTest.Migration;

public class NpgsqlMigrationTableCreator : IMigrationTableCreator<NpgsqlDbContext>
{
    private readonly ILogger<NpgsqlMigrationTableCreator> _logger;

    public NpgsqlMigrationTableCreator(ILogger<NpgsqlMigrationTableCreator> logger)
    {
        _logger = logger;
    }

    public Task CreateUpgradeTableIfNotExist(NpgsqlDbContext dbContext)
    {
        var result = dbContext.Database.ExecuteScalar(
            $"CREATE TABLE IF NOT EXISTS {RunTimeConfig.NpgsqlConfig.DataUpgradeTableName}(serviceKey text not null " +
            ", isRepeat bool not null ,executedTime timestamp with time zone)");
        if (result == null || (long)result == 0)
        {
            _logger.LogWarning("Create upgrade table failed");
        }

        return Task.CompletedTask;
    }

    public Task UpdateUpgradeTable(NpgsqlDbContext dbContext)
    {
        return Task.CompletedTask;
    }

    public Task CreateMigrationTableIfNotExist(NpgsqlDbContext dbContext)
    {
        var result = dbContext.Database.ExecuteScalar(
            $"CREATE TABLE IF NOT EXISTS \"{RunTimeConfig.NpgsqlConfig.MigrationTableName}\"(id uuid primary key not null ,runTime timestamp with time zone not null ,metadata text not null ,migrationId text not null ,productVersion text not null ,upOperations text not null ,downOperations text not null ,dbContextFullName text not null ,ingoretables text);");

        if (result == null || (long)result == 0)
        {
            _logger.LogWarning("Create migration history table failed");
        }

        return Task.CompletedTask;
    }

    public Task UpdateMigrationTable(NpgsqlDbContext dbContext)
    {
        return Task.CompletedTask;
    }
}