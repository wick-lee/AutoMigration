using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Wick.AutoMigration.Interface;

public interface IEnsureDbOperation
{
    public Task BeforeEnsureAsync(DbContext dbContext, DbConnection connection, IRelationalDatabaseCreator dbCreator);

    public Task AfterEnsureAsync(DbContext dbContext);
}