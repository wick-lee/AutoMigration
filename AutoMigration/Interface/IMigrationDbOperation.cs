using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Wick.AutoMigration.Enums;
using Wick.AutoMigration.Model;

namespace Wick.AutoMigration.Interface;

/// <summary>
///     Migration db required functions
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public interface IMigrationDbOperation<in TDbContext> where TDbContext : DbContext
{
    /// <summary>
    ///     Filter special migration operation
    /// </summary>
    public Func<MigrationOperation, SqlCommandType, bool>? FilterMigrationOperation { get; }

    /// <summary>
    ///     Provide the necessary assemblies for compiling the efcore snapshot model (eg. MySQL assembly is required when using
    ///     MySQL database)
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Assembly>? CompileSnapshotAssemblies();

    /// <summary>
    ///     Before migration operation.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="model"></param>
    /// <param name="recordModel"></param>
    /// <returns></returns>
    public Task BeforeMigrationOperationAsync(TDbContext dbContext, IRelationalModel? model,
        MigrationRecordModel? recordModel);

    /// <summary>
    ///     Get latest migration record.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public Task<MigrationRecordModel?> GetLastMigrationRecord(TDbContext dbContext);

    /// <summary>
    ///     Some types cannot be found when compiling ModelSnapshots and can be replaced with specific types.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public Task<string> ReplaceSpecialType(string source);

    /// <summary>
    ///     After generate migration commands, process commands
    /// </summary>
    /// <param name="migrationCommands"></param>
    /// <param name="commandType"></param>
    /// <returns></returns>
    public Task HandleMigrationCommand(IEnumerable<MigrationCommand> migrationCommands, SqlCommandType commandType);

    /// <summary>
    ///     Add migration record after auto migration
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="recordModel"></param>
    /// <returns></returns>
    public Task AddMigrationRecord(TDbContext dbContext, MigrationRecordModel recordModel);

    /// <summary>
    ///     Ignore update tables name.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string>? GetIgnoreTables();
}