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
    /// <param name="tableCreator">migration table creator</param>
    /// <param name="dbOperation">required migration operation <see cref="IMigrationDbOperation{TDbContext}" /></param>
    /// <typeparam name="TDbContext">Base on <see cref="DbContext" /></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddAutoMigration<TDbContext>(this IServiceCollection serviceCollection,
        Type tableCreator, Type dbOperation)
        where TDbContext : DbContext
    {
        return serviceCollection.AddSingleton<AutoMigration<TDbContext>>()
            .AddMigrationRequiredService<TDbContext>(tableCreator, dbOperation);
    }

    /// <summary>
    ///     Add auto migration and read config from configuration
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="tableCreator">required table creator</param>
    /// <param name="dbOperation">required migration operation</param>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <typeparam name="TDbContext">Base on <see cref="DbContext" /></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddAutoMigration<TDbContext>(this IServiceCollection serviceCollection,
        Type tableCreator, Type dbOperation, IConfiguration configuration, string sectionName = "AutoMigration")
        where TDbContext : DbContext
    {
        return serviceCollection.AddMigrationRequiredService<TDbContext>(tableCreator, dbOperation)
            .AddSingleton(provider => new AutoMigration<TDbContext>(provider, configuration.GetSection(sectionName)));
    }

    /// <summary>
    ///     Add auto migration, read config from configuration and add data upgrade service (with upgrade service assemblies)
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="tableCreator">required sql provider</param>
    /// <param name="dbOperation">required migration operation</param>
    /// <param name="upgradeAssemblies"></param>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <param name="upgradeServiceAssemblies"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddAutoMigration<TDbContext>(this IServiceCollection serviceCollection,
        Type tableCreator, Type dbOperation, Func<IEnumerable<Assembly>> upgradeAssemblies,
        IConfiguration configuration,
        string sectionName = "AutoMigration", params Assembly[] upgradeServiceAssemblies)
        where TDbContext : DbContext
    {
        return serviceCollection.AddSingleton(provider =>
                new AutoMigration<TDbContext>(provider, configuration.GetSection(sectionName), upgradeAssemblies))
            .AddUpgradeService(upgradeServiceAssemblies)
            .AddMigrationRequiredService<TDbContext>(tableCreator, dbOperation);
    }

    public static IServiceCollection AddAutoMigration<TDbContext, TMigrationTableCreator, TMigrationDbOperation>(
        this IServiceCollection serviceCollection)
        where TDbContext : DbContext
        where TMigrationTableCreator : IMigrationTableCreator<TDbContext>
        where TMigrationDbOperation : IMigrationDbOperation<TDbContext>
    {
        return serviceCollection.AddSingleton<AutoMigration<TDbContext>>()
            .AddMigrationRequiredService<TDbContext>(typeof(TMigrationTableCreator), typeof(TMigrationDbOperation));
    }

    public static IServiceCollection AddAutoMigration<TDbContext, TMigrationTableCreator, TMigrationDbOperation>(
        this IServiceCollection serviceCollection, Func<IEnumerable<Assembly>> upgradeAssemblies,
        IConfiguration configuration,
        string sectionName = "AutoMigration", params Assembly[] upgradeServiceAssemblies)
        where TDbContext : DbContext
        where TMigrationTableCreator : IMigrationTableCreator<TDbContext>
        where TMigrationDbOperation : IMigrationDbOperation<TDbContext>
    {
        return serviceCollection.AddSingleton<AutoMigration<TDbContext>>(provider =>
                new AutoMigration<TDbContext>(provider, configuration.GetSection(sectionName), upgradeAssemblies))
            .AddUpgradeService(upgradeServiceAssemblies)
            .AddMigrationRequiredService<TDbContext>(typeof(TMigrationTableCreator), typeof(TMigrationDbOperation));
    }

    private static IServiceCollection AddMigrationRequiredService<TDbContext>(this IServiceCollection serviceCollection,
        Type tableCreator, Type dbOperation) where TDbContext : DbContext
    {
        serviceCollection.AddSingleton(typeof(IMigrationTableCreator<TDbContext>), tableCreator);
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