using System.Data;
using System.Reflection;
using AutoMigration.WebTest.Entities;
using AutoMigration.WebTest.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using MySqlConnector;
using Npgsql;
using Wick.AutoMigration.Enums;
using Wick.AutoMigration.Interface;
using Wick.AutoMigration.Model;

namespace AutoMigration.WebTest.Migration;

public class MysqlMigrationDbOperation : IMigrationDbOperation<MysqlDbContext>
{
    private readonly ILogger<MysqlMigrationDbOperation> _logger;

    public MysqlMigrationDbOperation(ILogger<MysqlMigrationDbOperation> logger)
    {
        _logger = logger;
    }

    public IEnumerable<Assembly> CompileSnapshotAssemblies()
    {
        return new List<Assembly>()
        {
            typeof(TestEntity).Assembly,
            typeof(MySqlMigrationBuilderExtensions).Assembly,
            typeof(MySqlValueGenerationStrategy).Assembly
        };
    }

    public Task BeforeMigrationOperationAsync(MysqlDbContext dbContext, IRelationalModel? model,
        MigrationRecordModel? recordModel)
    {
        return Task.CompletedTask;
    }

    public Task<MigrationRecordModel?> GetLastMigrationRecord(MysqlDbContext dbContext)
    {
        var sql =
            $"select productversion, migrationid, metadata, dbcontextfullname from {RunTimeConfig.MysqlConfig.MigrationTableName} where dbcontextfullname = @dbcontextfullname order by runtime desc limit 1;";
        return GetRecord(dbContext, sql, new MySqlParameter("@dbcontextfullname", dbContext.GetType().FullName));
    }

    public Task<string> ReplaceSpecialType(string source)
    {
        return Task.FromResult(source);
    }

    public Func<MigrationOperation, SqlCommandType, bool>? FilterMigrationOperation { get; } = null;

    public Task HandleMigrationCommand(IEnumerable<MigrationCommand> migrationCommands, SqlCommandType commandType)
    {
        return Task.CompletedTask;
    }

    public async Task AddMigrationRecord(MysqlDbContext dbContext, MigrationRecordModel recordModel)
    {
        var sql =
            $"insert into {RunTimeConfig.MysqlConfig.MigrationTableName} ( runtime, metadata, migrationid, productversion, upoperations, downoperations, dbcontextfullname, ingoretables)" +
            "values (@runtime, @metadata, @migrationid, @productversion, @upoperations, @downoperations, @dbcontextfullname, @ingoretables);";

        var result = await dbContext.Database.ExecuteSqlRawAsync(sql, new List<object>()
        {
            // new MySqlParameter("@id", recordModel.Id),
            new MySqlParameter("@runtime", recordModel.RunTime),
            new MySqlParameter("@metadata", recordModel.Metadata),
            new MySqlParameter("@migrationid", recordModel.MigrationId),
            new MySqlParameter("@productversion", recordModel.ProductVersion),
            new MySqlParameter("@upoperations", recordModel.UpOperations),
            new MySqlParameter("@downoperations", recordModel.DownOperations),
            new MySqlParameter("@dbcontextfullname", recordModel.DbContextFullName),
            new MySqlParameter("@ingoretables",
                string.IsNullOrWhiteSpace(recordModel.IgnoreTables) ? DBNull.Value : recordModel.IgnoreTables)
        });

        if (result != 1)
        {
            _logger.LogWarning("Insert migration history record failed");
        }
    }

    public IEnumerable<string>? GetIgnoreTables()
    {
        return null;
    }

    public Task AddUpgradeRecord(MysqlDbContext dbContext, IDataUpgradeService upgradeService)
    {
        var sql =
            $"INSERT INTO \"{RunTimeConfig.NpgsqlConfig.DataUpgradeTableName}\" (serviceKey, isRepeat, executedTime) VALUES ('{upgradeService.Key}','{upgradeService.IsRepeat}','{DateTimeOffset.UtcNow}');";
        var result = dbContext.Database.ExecuteSqlRaw(sql);
        if (result != 1)
        {
            _logger.LogWarning("Add upgrade record failed");
        }

        return Task.CompletedTask;
    }

    public Task<bool> CheckRunUpgradeService(MysqlDbContext dbContext, IDataUpgradeService upgradeService)
    {
        if (upgradeService.IsRepeat)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult((long)dbContext.Database.ExecuteScalar(
            $"SELECT count(1) FROM tableName WHERE \"serviceKey\" = '{upgradeService.Key}'") == 0);
    }

    private static async Task<MigrationRecordModel?> GetRecord(MysqlDbContext dbContext, string sql,
        params object[]? sqlObjects)
    {
        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        if (sqlObjects?.Any() == true)
        {
            command.Parameters.AddRange(sqlObjects);
        }

        await using var result = await command.ExecuteReaderAsync();
        if (await result.ReadAsync())
        {
            return new MigrationRecordModel(result.GetString(2), result.GetString(1), result.GetString(0), "", "",
                result.GetString(3));
        }

        return null;
    }
}