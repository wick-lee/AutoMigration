using System.Data;
using System.Reflection;
using AutoMigration.WebTest.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Wick.AutoMigration.Config;
using Wick.AutoMigration.Enums;
using Wick.AutoMigration.Interface;
using Wick.AutoMigration.Model;

namespace AutoMigration.WebTest.Migration;

public class NpgsqlMigrationDbOperation : IMigrationDbOperation<MyDbContext>
{
    private readonly ILogger<NpgsqlMigrationDbOperation> _logger;

    public NpgsqlMigrationDbOperation(ILogger<NpgsqlMigrationDbOperation> logger)
    {
        _logger = logger;
    }

    public IEnumerable<Assembly> CompileSnapshotAssemblies()
    {
        return new List<Assembly>
        {
            typeof(NpgsqlValueGenerationStrategy).Assembly,
            typeof(NpgsqlIndexBuilderExtensions).Assembly,
            typeof(TestEntity).Assembly
        };
    }

    public Task BeforeMigrationOperationAsync(MyDbContext dbContext, IRelationalModel? model,
        MigrationRecordModel? recordModel)
    {
        return Task.CompletedTask;
    }

    public Task<MigrationRecordModel?> GetLastMigrationRecord(MyDbContext dbContext)
    {
        var sql =
            $"select productversion, migrationid, metadata, dbcontextfullname from {AutoMigrationConfig.MigrationTableName} where dbcontextfullname = @dbcontextfullname order by runtime desc limit 1;";
        return GetRecord(dbContext, sql, new NpgsqlParameter("@dbcontextfullname", dbContext.GetType().FullName));
    }

    public Task<string> ReplaceSpecialType(string source)
    {
        return Task.FromResult(source);
    }

    public Func<MigrationOperation, SqlCommandType, bool>? FilterMigrationOperation => null;

    public Task HandleMigrationCommand(IEnumerable<MigrationCommand> migrationCommands, SqlCommandType commandType)
    {
        return Task.CompletedTask;
    }

    public async Task AddMigrationRecord(MyDbContext dbContext, MigrationRecordModel recordModel)
    {
        var sql =
            $"insert into {AutoMigrationConfig.MigrationTableName} (id, runtime, metadata, migrationid, productversion, upoperations, downoperations, dbcontextfullname, ingoretables)" +
            "values (@id, @runtime, @metadata, @migrationid, @productversion, @upoperations, @downoperations, @dbcontextfullname, @ingoretables);";

        var result = await dbContext.Database.ExecuteSqlRawAsync(sql, new List<object>()
        {
            new NpgsqlParameter("@id", recordModel.Id),
            new NpgsqlParameter("@runtime", recordModel.RunTime),
            new NpgsqlParameter("@metadata", recordModel.Metadata),
            new NpgsqlParameter("@migrationid", recordModel.MigrationId),
            new NpgsqlParameter("@productversion", recordModel.ProductVersion),
            new NpgsqlParameter("@upoperations", recordModel.UpOperations),
            new NpgsqlParameter("@downoperations", recordModel.DownOperations),
            new NpgsqlParameter("@dbcontextfullname", recordModel.DbContextFullName),
            new NpgsqlParameter("@ingoretables",
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

    private static async Task<MigrationRecordModel?> GetRecord(MyDbContext dbContext, string sql,
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