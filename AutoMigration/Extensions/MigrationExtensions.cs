using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Wick.AutoMigration.Exceptions;

namespace Wick.AutoMigration.Extensions;

internal static class MigrationExtensions
{
    internal static async Task CreateTableIfNotExistAsync(this DbContext dbContext, string checkTableExistSql,
        string createTableSql, string tableName)
    {
        if (string.IsNullOrWhiteSpace(checkTableExistSql))
        {
            throw new MigrationException($"Check table exist sql is empty. Table name: {tableName}");
        }

        if ((long)(dbContext.Database.ExecuteScalar(checkTableExistSql) ?? 1) == 0)
        {
            if (string.IsNullOrWhiteSpace(createTableSql))
            {
                throw new MigrationException($"The create table sql script is empty. Table name: {tableName}");
            }

            await dbContext.Database.ExecuteSqlRawAsync(createTableSql);
        }
    }

    internal static Task<int> ExecutedSqlRawAsync(this DbContext dbContext, string sql,
        IEnumerable<object>? objects = null)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new MigrationException("Sql can not be empty.");
        }

        return dbContext.Database.ExecuteSqlRawAsync(sql, objects ?? new List<object>());
    }

    internal static object? ExecuteScalar(this DatabaseFacade databaseFacade, string sql,
        List<DbParameter>? dbParameters = null)
    {
        var connection = databaseFacade.GetDbConnection();
        using var command = connection.CreateCommand();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        if (dbParameters?.Any() == true)
        {
            command.Parameters.AddRange(dbParameters.ToArray());
        }

        command.CommandText = sql;
        command.CommandTimeout = 300;

        return command.ExecuteScalar();
    }
}