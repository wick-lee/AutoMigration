namespace Wick.AutoMigration.Config;

public class MigrationConfig
{
    /// <summary>
    /// Migration history table name.
    /// </summary>
    public string MigrationTableName { get; set; } = "migration_history";

    /// <summary>
    /// Data upgrade table name.
    /// </summary>
    public string DataUpgradeTableName { get; set; } = "data_upgrade_history";

    /// <summary>
    /// Run upgrade service
    /// </summary>
    public bool RunUpgradeService { get; set; } = true;

    /// <summary>
    /// Stop migration db after ensure and create db.
    /// </summary>
    public bool StopMigrationAfterEnsureDb { get; set; } = true;

    /// <summary>
    /// SQL statement separator
    /// </summary>
    public string SqlStatementSeparator { get; set; } = ",";
}