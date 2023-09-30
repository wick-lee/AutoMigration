using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AutoMigration.WebTest.Extensions;

public static class DbContextExtensions
{
    internal static object? ExecuteScalar(this DatabaseFacade databaseFacade, string sql,
        List<DbParameter>? dbParameters = null)
    {
        var connection = databaseFacade.GetDbConnection();
        using var command = connection.CreateCommand();
        if (connection.State != ConnectionState.Open) connection.Open();

        if (dbParameters?.Any() == true) command.Parameters.AddRange(dbParameters.ToArray());

        command.CommandText = sql;
        command.CommandTimeout = 300;

        return command.ExecuteScalar();
    }
}