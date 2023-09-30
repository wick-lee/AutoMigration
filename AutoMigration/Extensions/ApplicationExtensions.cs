using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Wick.AutoMigration.Extensions;

public static class ApplicationExtensions
{
    public static IApplicationBuilder RunAutoMigration<TDbContext>(this IApplicationBuilder app)
        where TDbContext : DbContext
    {
        using var serviceProvider = app.ApplicationServices.CreateScope();
        var autoMigration = serviceProvider.ServiceProvider.GetRequiredService<AutoMigration<TDbContext>>();
        autoMigration.MigrationDbAsync().GetAwaiter().GetResult();

        return app;
    }
}