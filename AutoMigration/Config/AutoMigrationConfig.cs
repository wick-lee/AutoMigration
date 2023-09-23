namespace Wick.AutoMigration.Config;

public static class AutoMigrationConfig
{
    public static MigrationConfig? MigrationConfig { get; set; } = new MigrationConfig();

    public static string MigrationTableName => MigrationConfig.MigrationTableName;

    public static string DataUpgradeTableName => MigrationConfig.DataUpgradeTableName;

    public static bool RunUpgradeService => MigrationConfig.RunUpgradeService;

    public static bool StopMigrationAfterEnsureDb => MigrationConfig.StopMigrationAfterEnsureDb;

    public static string SqlStatementSeparator => MigrationConfig.SqlStatementSeparator;
}