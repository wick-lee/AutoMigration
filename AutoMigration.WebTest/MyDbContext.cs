using AutoMigration.WebTest.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutoMigration.WebTest;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}