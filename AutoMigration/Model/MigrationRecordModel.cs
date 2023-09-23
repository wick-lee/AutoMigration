namespace Wick.AutoMigration.Model;

public class MigrationRecordModel
{
    public Guid Id { get; set; }
    public DateTimeOffset RunTime { get; set; }
    public string Metadata { get; set; }
    public string MigrationId { get; set; }
    public string ProductVersion { get; set; }
    public string UpOperations { get; set; }
    public string DownOperations { get; set; }
    public string DbContextFullName { get; set; }
    public string? IgnoreTables { get; set; }

    public MigrationRecordModel(string metadata, string migrationId,
        string productVersion, string upOperations, string downOperations, string dbContextFullName,
        string? ignoreTables = null, DateTimeOffset? runTime = null, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        RunTime = runTime ?? DateTimeOffset.UtcNow;
        Metadata = metadata;
        MigrationId = migrationId;
        ProductVersion = productVersion;
        UpOperations = upOperations;
        DownOperations = downOperations;
        DbContextFullName = dbContextFullName;
        IgnoreTables = ignoreTables;
    }
}