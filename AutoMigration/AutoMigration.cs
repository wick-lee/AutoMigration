using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wick.AutoMigration.Config;
using Wick.AutoMigration.Enums;
using Wick.AutoMigration.Exceptions;
using Wick.AutoMigration.Helper;
using Wick.AutoMigration.Interface;
using Wick.AutoMigration.Model;

namespace Wick.AutoMigration;

public class AutoMigration<TDbContext> where TDbContext : DbContext
{
    private readonly MigrationConfig _config = new();
    private readonly IEnsureDbOperation? _ensureDbOperation;
    private readonly ILogger _logger;
    private readonly IMigrationDbOperation<TDbContext> _migrationDbOperation;
    private readonly IMigrationTableCreator<TDbContext> _migrationTableCreator;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<Assembly>? _upgradeAssemblies;

    public AutoMigration(IServiceProvider serviceProvider, IConfiguration? configuration,
        Func<IEnumerable<Assembly>> upgradeAssemblies) : this(serviceProvider, configuration)
    {
        _upgradeAssemblies = upgradeAssemblies.Invoke();
    }

    public AutoMigration(IServiceProvider serviceProvider, IConfiguration? configuration = null)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<AutoMigration<TDbContext>>>();
        _ensureDbOperation = _serviceProvider.GetService<IEnsureDbOperation>();
        _migrationTableCreator = _serviceProvider.GetRequiredService<IMigrationTableCreator<TDbContext>>();
        _migrationDbOperation = _serviceProvider.GetRequiredService<IMigrationDbOperation<TDbContext>>();
        configuration?.Bind(_config);
    }

    /// <summary>
    ///     Ensure db and run upgrade service and migration db.
    /// </summary>
    /// <exception cref="MigrationException"></exception>
    public async Task MigrationDbAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            var dbEnsured = await EnsureDb(dbContext, _ensureDbOperation);

            if (_config.RunUpgradeService)
            {
                await RunUpgradeService(_upgradeAssemblies);
            }

            if (dbEnsured && _config.StopMigrationAfterEnsureDb)
            {
                _logger.LogInformation("Db has been ensure and all table is created, skip migration db");
                await AddDesignTimeSnapshot(dbContext);
            }
            else
            {
                await RunMigration(dbContext);
            }

            if (_config.RunUpgradeService)
                await RunUpgradeService(_upgradeAssemblies, MigrationRuntimeType.AfterMigration);
        }
        catch (Exception ex)
        {
            _logger.LogError("EnsureDbAndMigration failed. Error message: {Ex}", ex);
            throw new MigrationException("EnsureDbAndMigration failed.", ex);
        }
    }

    /// <summary>
    ///     Run upgrade services
    /// </summary>
    /// <param name="assemblies">if assemblies it not null, only upgrade assemblies data upgrade services.</param>
    /// <param name="migrationRuntimeType"></param>
    public async Task RunUpgradeService(IEnumerable<Assembly>? assemblies = null,
        MigrationRuntimeType migrationRuntimeType = MigrationRuntimeType.BeforeMigration)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        _logger.LogInformation("Start create upgrade table if not exist");
        await _migrationTableCreator.CreateUpgradeTableIfNotExist(dbContext);

        _logger.LogInformation("Start update upgrade table");
        await _migrationTableCreator.UpdateUpgradeTable(dbContext);

        var dataUpgradeServices =
            (scope.ServiceProvider.GetService<IEnumerable<IDataUpgradeService>>() ?? Array.Empty<IDataUpgradeService>())
            .Where(service => assemblies?.Contains(service.GetType().Assembly) ??
                              service.MigrationRuntimeType == migrationRuntimeType);

        await DoRunUpgradeService(dbContext, dataUpgradeServices);
    }

    private async Task DoRunUpgradeService(TDbContext dbContext, IEnumerable<IDataUpgradeService> upgradeServices)
    {
        foreach (var upgradeService in upgradeServices)
        {
            if (!await _migrationDbOperation.CheckRunUpgradeService(dbContext, upgradeService))
            {
                _logger.LogDebug("Upgrade service {Key} skip executed", upgradeService.Key);
                continue;
            }

            try
            {
                await upgradeService.OperationAsync();
                await _migrationDbOperation.AddUpgradeRecord(dbContext, upgradeService);
            }
            catch (Exception e)
            {
                _logger.LogError("Upgrade service {Key} running failed. Exception: {E}", upgradeService.Key, e);
            }
        }
    }

    /// <summary>
    ///     Ensure db.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="operation">The operation between ensure db.</param>
    /// <exception cref="MigrationException"></exception>
    public async Task<bool> EnsureDb(TDbContext dbContext, IEnsureDbOperation? operation = null)
    {
        var connection = dbContext.Database.GetDbConnection();
        bool result;

        try
        {
            var dbCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
            if (dbCreator is null)
            {
                _logger.LogError("Db creator is not exist");
                throw new MigrationException("Db creator is not exist");
            }

            if (operation != null) await operation.BeforeEnsureAsync(dbContext, connection, dbCreator);

            result = await dbCreator.EnsureCreatedAsync();

            _logger.LogInformation("Ensure db finished. db: {Database}", connection.Database);

            if (operation != null) await operation.AfterEnsureAsync(dbContext);
        }
        catch (Exception e)
        {
            _logger.LogError("EnsureDb failed. Error message {Ex}", e);
            throw new MigrationException("EnsureDb failed.", e);
        }

        return result;
    }

    /// <summary>
    ///     Run migration
    /// </summary>
    /// <param name="dbContext"></param>
    /// <exception cref="MigrationException"></exception>
    public async Task RunMigration(TDbContext dbContext)
    {
        var migrationAssembly = dbContext.GetService<IMigrationsAssembly>();
        var designTimeModel = dbContext.GetService<IDesignTimeModel>();
        var dbService = BuildScaffolderDependencies(dbContext, migrationAssembly);
        var dependencies = dbService.GetRequiredService<MigrationsScaffolderDependencies>();
        var migrationId = dependencies.MigrationsIdGenerator.GenerateId(dbContext.GetType().Name);

        var assemblies = GetMigrationAssemblies();
        assemblies.Add(migrationAssembly.Assembly);

        await UpdateMigrationHistoryTable(dbContext);

        var lastMigrationRecord = await _migrationDbOperation.GetLastMigrationRecord(dbContext);
        var snapshot = migrationAssembly.ModelSnapshot;
        if (lastMigrationRecord != null)
        {
            lastMigrationRecord.Metadata = await _migrationDbOperation.ReplaceSpecialType(lastMigrationRecord.Metadata);
            try
            {
                snapshot = MigrationHelper.CompileSnapshot<ModelSnapshot>(assemblies, lastMigrationRecord.Metadata);
            }
            catch (Exception e)
            {
                throw new MigrationException("Compile model snapshot failed.", e);
            }
        }

        var snapshotModel = GetSnapshotModel(snapshot, dbContext, dependencies);
        IRelationalModel? oldModel = snapshotModel?.GetRelationalModel(),
            newModel = designTimeModel.Model.GetRelationalModel();
        IEnumerable<MigrationCommand> upMigrationCommands, unDoMigrationCommands;
        var ignoreTables = RemoveIgnoredTable(oldModel, newModel);
        await _migrationDbOperation.BeforeMigrationOperationAsync(dbContext, snapshotModel?.GetRelationalModel(),
            lastMigrationRecord);

        try
        {
            upMigrationCommands = await GetMigrationCommand(oldModel, newModel, dependencies, dbContext,
                SqlCommandType.UpSqlCommand);

            unDoMigrationCommands = await GetMigrationCommand(newModel, oldModel, dependencies, dbContext,
                SqlCommandType.DownSqlCommand);
        }
        catch (Exception e)
        {
            _logger.LogError("Generate migration command failed");
            throw new MigrationException("Generate migration command failed", e);
        }

        string upSqls = string.Join(_config.SqlStatementSeparator,
                upMigrationCommands.Select(c => c.CommandText)),
            downSqls = string.Join(_config.SqlStatementSeparator,
                unDoMigrationCommands.Select(c => c.CommandText));

        if (upMigrationCommands.Any())
        {
            try
            {
                var commandExecutor = dbContext.GetService<IMigrationCommandExecutor>();
                await commandExecutor.ExecuteNonQueryAsync(upMigrationCommands,
                    dbContext.GetService<IRelationalConnection>());
            }
            catch (Exception e)
            {
                _logger.LogError("Execute up migration commend failed, error message: {E}", e);
                throw new MigrationException("Execute up migration commend failed", e);
            }
        }
        else
        {
            _logger.LogInformation("No up migration operation");
        }


        var migrationRecordModel = new MigrationRecordModel(GetCurrentSnapshotModelValue(dbContext, dependencies),
            migrationId, GetProductVersion(designTimeModel), upSqls, downSqls,
            dbContext.GetType().FullName ?? "Unknown full name", ignoreTables);

        await _migrationDbOperation.AddMigrationRecord(dbContext, migrationRecordModel);
    }

    private HashSet<Assembly> GetMigrationAssemblies()
    {
        var assemblies = _migrationDbOperation.CompileSnapshotAssemblies();
        if (assemblies != null && assemblies.Any())
            return MigrationHelper.DefaultMigrationAssemblies.Concat(assemblies).ToHashSet();

        return MigrationHelper.DefaultMigrationAssemblies.ToHashSet();
    }

    private async Task AddDesignTimeSnapshot(TDbContext dbContext)
    {
        var migrationAssembly = dbContext.GetService<IMigrationsAssembly>();
        var designTimeModel = dbContext.GetService<IDesignTimeModel>();
        var dbService = BuildScaffolderDependencies(dbContext, migrationAssembly);
        var dependencies = dbService.GetRequiredService<MigrationsScaffolderDependencies>();
        var migrationId = dependencies.MigrationsIdGenerator.GenerateId(dbContext.GetType().Name);
        var dRelationalModel = designTimeModel.Model.GetRelationalModel();
        var ignoreTables = RemoveIgnoredTable(null, dRelationalModel);

        await UpdateMigrationHistoryTable(dbContext);

        IEnumerable<MigrationCommand> upMigrationCommands;
        try
        {
            upMigrationCommands = await GetMigrationCommand(null, dRelationalModel, dependencies, dbContext,
                SqlCommandType.UpSqlCommand);
        }
        catch (Exception e)
        {
            _logger.LogError("Generate migration command failed");
            throw new MigrationException("Generate migration command failed", e);
        }

        var upSqls = string.Join(_config.SqlStatementSeparator,
            upMigrationCommands.Select(c => c.CommandText));

        var migrationRecordModel = new MigrationRecordModel(GetCurrentSnapshotModelValue(dbContext, dependencies),
            migrationId, GetProductVersion(designTimeModel), upSqls, string.Empty,
            dbContext.GetType().FullName ?? "Unknown full name", ignoreTables);

        await _migrationDbOperation.AddMigrationRecord(dbContext, migrationRecordModel);
    }

    private async Task<IEnumerable<MigrationCommand>> GetMigrationCommand(IRelationalModel? oldModel,
        IRelationalModel? newModel, MigrationsScaffolderDependencies dependencies, TDbContext dbContext,
        SqlCommandType commandType)
    {
        if (!dependencies.MigrationsModelDiffer.HasDifferences(oldModel, newModel))
            return new List<MigrationCommand>();

        var migrationOperations = dependencies.MigrationsModelDiffer.GetDifferences(oldModel, newModel)
            .Where(op => FilterMigrationOperation(op, commandType)).ToList();
        var sqlGenerator = dbContext.GetService<IMigrationsSqlGenerator>();
        var commands = sqlGenerator.Generate(migrationOperations, dbContext.Model);

        await _migrationDbOperation.HandleMigrationCommand(commands, commandType);

        return commands;
    }

    private async Task UpdateMigrationHistoryTable(TDbContext dbContext)
    {
        _logger.LogInformation("Start create migration history table if not exist");
        await _migrationTableCreator.CreateMigrationTableIfNotExist(dbContext);
        _logger.LogInformation("Start update migration history table");
        await _migrationTableCreator.UpdateMigrationTable(dbContext);
    }

    private bool FilterMigrationOperation(MigrationOperation operation, SqlCommandType commandType)
    {
        return _migrationDbOperation.FilterMigrationOperation == null ||
               _migrationDbOperation.FilterMigrationOperation(operation, commandType);
    }

    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    private static IModel? GetSnapshotModel(ModelSnapshot? snapshot, TDbContext dbContext,
        MigrationsScaffolderDependencies dependencies)
    {
        if (snapshot == null) return null;

        var result = snapshot.Model;
        if (result is IMutableModel mutableModel) result = mutableModel.FinalizeModel();

        result = dbContext.GetService<IModelRuntimeInitializer>().Initialize(result);
        result = dependencies.SnapshotModelProcessor.Process(result);

        return result;
    }

    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    private static IServiceProvider BuildScaffolderDependencies(TDbContext dbContext,
        IMigrationsAssembly migrationsAssembly)
    {
        var builder = new DesignTimeServicesBuilder(migrationsAssembly.Assembly,
            Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly(),
            new OperationReporter(new OperationReportHandler()), Array.Empty<string>());
        return builder.Build(dbContext);
    }

    private static string GetCurrentSnapshotModelValue(TDbContext dbContext,
        MigrationsScaffolderDependencies dependencies)
    {
        var migrationId = dependencies.MigrationsIdGenerator.GenerateId(dbContext.GetType().Name);
        var model = dbContext.GetService<IDesignTimeModel>().Model;
        var codeGenerator = dependencies.MigrationsCodeGeneratorSelector.Select(null);
        return codeGenerator.GenerateSnapshot(MethodBase.GetCurrentMethod()?.ReflectedType?.Namespace,
            dbContext.GetType(), $"Migration_{migrationId}", model);
    }

    private string? RemoveIgnoredTable(IRelationalModel? oldModel, IRelationalModel newModel)
    {
        var ignoreTables = _migrationDbOperation.GetIgnoreTables();
        if (ignoreTables == null || !ignoreTables.Any()) return null;

        var newModelProperties = newModel.GetType().GetProperties();
        foreach (var property in newModelProperties)
        {
            if (property.Name != "Tables" ||
                property.PropertyType != typeof(SortedDictionary<(string, string), Table>))
                continue;

            foreach (var table in ignoreTables.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                if (oldModel != null)
                {
                    (property.GetValue(oldModel) as SortedDictionary<(string, string), Table>)?.Remove((table, null));
                }

                (property.GetValue(newModel) as SortedDictionary<(string, string), Table>)?.Remove((table, null));
            }

            break;
        }

        return string.Join(",\r\n", ignoreTables);
    }

    private static string GetProductVersion(IDesignTimeModel? model)
    {
        if (model is null)
            return "Unknown version";

        return model.Model.FindAnnotation("ProductVersion")?.Value?.ToString() ?? "Unknown version";
    }
}