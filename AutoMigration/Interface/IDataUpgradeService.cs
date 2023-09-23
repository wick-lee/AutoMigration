using Wick.AutoMigration.Enums;

namespace Wick.AutoMigration.Interface;

/// <summary>
/// Before migration, upgrade data service.
/// </summary>
public interface IDataUpgradeService
{
    public string Key { get; }

    public bool IsRepeat { get; }

    public MigrationRuntimeType MigrationRuntimeType { get; }

    public Task OperationAsync();
}