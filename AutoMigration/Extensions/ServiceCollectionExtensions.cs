using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wick.AutoMigration.Config;
using Wick.AutoMigration.Interface;

namespace Wick.AutoMigration.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add auto migration with default config
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="sqlProvider">required sql provider <see cref="IMigrationSqlProvider{TDbContext}"/></param>
    /// <param name="dbOperation">required migration operation <see cref="IMigrationDbOperation{TDbContext}"/></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddAutoMigration<TDbContext>(this IServiceCollection serviceCollection,
        Type sqlProvider, Type dbOperation)
        where TDbContext : DbContext
    {
        serviceCollection.AddSingleton<AutoMigration<TDbContext>>();
        serviceCollection.AddSingleton(typeof(IMigrationSqlProvider<TDbContext>), sqlProvider);
        serviceCollection.AddSingleton(typeof(IMigrationDbOperation<TDbContext>), dbOperation);
        return serviceCollection;
    }

    /// <summary>
    /// Add auto migration and read config from configuration
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="sqlProvider">required sql provider</param>
    /// <param name="dbOperation">required migration operation</param>
    /// <param name="configuration"></param>
    /// <param name="configSection"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <see cref="IMigrationDbOperation{TDbContext}"/>
    /// <see cref="IMigrationSqlProvider{TDbContext}"/>
    /// <returns></returns>
    public static IServiceCollection AddAutoMigration<TDbContext>(this IServiceCollection serviceCollection,
        Type sqlProvider, Type dbOperation, IConfiguration configuration, string configSection = "AutoMigration")
        where TDbContext : DbContext
    {
        serviceCollection.AddAutoMigration<TDbContext>(sqlProvider, dbOperation);
        configuration.GetSection(configSection).Bind(AutoMigrationConfig.MigrationConfig);
        return serviceCollection;
    }
}