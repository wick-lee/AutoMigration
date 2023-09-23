using Microsoft.EntityFrameworkCore;
using Wick.AutoMigration.Model;

namespace Wick.AutoMigration.Interface;

/// <summary>
/// Provider for run upgrade service and migration db sql.
/// </summary>
/// <typeparam name="TDbContext">Add for specific DbContext</typeparam>
public interface IMigrationSqlProvider<in TDbContext> where TDbContext : DbContext
{
    /// <summary>
    /// Create upgrade service history table script.
    /// </summary>
    /// <returns></returns>
    public string CreateUpgradeHistoryTableScript();

    /// <summary>
    /// Sql script for check upgrade service history table is exist. 
    /// </summary>
    /// <returns>after execution should return number, if greater than 0 the table already exists.</returns>
    public string CheckUpgradeHistoryTableExistScript();

    /// <summary>
    /// Update upgrade history table, if no need it return null.
    /// </summary>
    /// <returns></returns>
    public string UpdateUpgradeHistoryTableSql();

    /// <summary>
    /// Check upgrade service has executed.
    /// </summary>
    /// <param name="upgradeService"></param>
    /// <returns></returns>
    public string CheckUpgradeServiceHasExecutedSql(IDataUpgradeService upgradeService);

    /// <summary>
    /// Get add upgrade service record sql.
    /// </summary>
    /// <param name="upgradeService"></param>
    /// <returns></returns>
    public string GetAddUpgradeRecordSql(IDataUpgradeService upgradeService);

    /// <summary>
    /// Sql script for check migration history table is exist.
    /// </summary>
    /// <returns>after execution should return number, if greater than 0 the table already exists.</returns>
    public string CheckMigrationHistoryTableExistScript();

    /// <summary>
    /// Create migration history table.
    /// </summary>
    /// <returns></returns>
    public string CreateMigrationHistoryTableScript();
    
    /// <summary>
    /// Update migration history table.
    /// </summary>
    /// <returns></returns>
    public string UpdateMigrationHistoryTableSql();
}