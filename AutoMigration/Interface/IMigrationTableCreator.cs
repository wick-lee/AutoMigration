using Microsoft.EntityFrameworkCore;

namespace Wick.AutoMigration.Interface;

/// <summary>
///     Auto migration table creator.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public interface IMigrationTableCreator<in TDbContext> where TDbContext : DbContext
{
    /// <summary>
    ///     Create upgrade table if not exist.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public Task CreateUpgradeTableIfNotExist(TDbContext dbContext);

    /// <summary>
    ///     Update upgrade table if needed.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public Task UpdateUpgradeTable(TDbContext dbContext);

    /// <summary>
    ///     Create migration table if not exist.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public Task CreateMigrationTableIfNotExist(TDbContext dbContext);

    /// <summary>
    ///     Update migration table if needed.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public Task UpdateMigrationTable(TDbContext dbContext);
}