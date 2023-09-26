using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wick.AutoMigration.Helper;
using Wick.AutoMigration.Interface;

namespace Wick.AutoMigration.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add auto migration with default config
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="sqlProvider">required sql provider <see cref="IMigrationSqlProvider{TDbContext}" /></param>
    /// <param name="dbOperation">required migration operation <see cref="IMigrationDbOperation{TDbContext}" /></param>
    /// <typeparam name="TDbContext">Base on <see cref="DbContext" /></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddAutoMigration<TDbContext>(this IServiceCollection serviceCollection,
        Type sqlProvider, Type dbOperation)
        where TDbContext : DbContext
    {
        return serviceCollection.AddSingleton<AutoMigration<TDbContext>>()
            .AddMigrationRequiredService<TDbContext>(sqlProvider, dbOperation);
    }

    /// <summary>
    ///     Add auto migration and read config from configuration
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="sqlProvider">required sql provider</param>
    /// <param name="dbOperation">required migration operation</param>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <typeparam name="TDbContext">Base on <see cref="DbContext" /></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddAutoMigration<TDbContext>(this IServiceCollection serviceCollection,
        Type sqlProvider, Type dbOperation, IConfiguration configuration, string sectionName = "AutoMigration")
        where TDbContext : DbContext
    {
        return serviceCollection.AddMigrationRequiredService<TDbContext>(sqlProvider, dbOperation)
            .AddSingleton(provider => new AutoMigration<TDbContext>(provider, configuration.GetSection(sectionName)));
    }

    /// <summary>
    ///     Add auto migration, read config from configuration and add data upgrade service (with upgrade service assemblies)
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="sqlProvider">required sql provider</param>
    /// <param name="dbOperation">required migration operation</param>
    /// <param name="upgradeAssemblies"></param>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <param name="upgradeServiceAssemblies"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddAutoMigration<TDbContext>(this IServiceCollection serviceCollection,
        Type sqlProvider, Type dbOperation, Func<IEnumerable<Assembly>> upgradeAssemblies, IConfiguration configuration,
        string sectionName = "AutoMigration", params Assembly[] upgradeServiceAssemblies)
        where TDbContext : DbContext
    {
        return serviceCollection.AddSingleton(provider =>
                new AutoMigration<TDbContext>(provider, configuration.GetSection(sectionName), upgradeAssemblies))
            .AddUpgradeService(upgradeServiceAssemblies)
            .AddMigrationRequiredService<TDbContext>(sqlProvider, dbOperation);
    }

    private static IServiceCollection AddMigrationRequiredService<TDbContext>(this IServiceCollection serviceCollection,
        Type sqlProvider, Type dbOperation) where TDbContext : DbContext
    {
        serviceCollection.AddSingleton(typeof(IMigrationSqlProvider<TDbContext>), sqlProvider);
        serviceCollection.AddSingleton(typeof(IMigrationDbOperation<TDbContext>), dbOperation);

        return serviceCollection;
    }

    private static IServiceCollection AddUpgradeService(this IServiceCollection serviceCollection,
        params Assembly[] assemblies)
    {
        foreach (var type in AssemblyHelper.GetImplementTypes(typeof(IDataUpgradeService), assemblies) ??
                             new List<Type>())
            serviceCollection.AddTransient(typeof(IDataUpgradeService), type);

        return serviceCollection;
    }
}