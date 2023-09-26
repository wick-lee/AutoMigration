using Wick.AutoMigration.Config;

namespace AutoMigration.WebTest;

public static class RunTimeConfig
{
    public static MigrationConfig NpgsqlConfig { get; set; } = new();
    public static MigrationConfig MysqlConfig { get; set; } = new();
}