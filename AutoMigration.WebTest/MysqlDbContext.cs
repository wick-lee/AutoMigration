using AutoMigration.WebTest.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoMigration.WebTest;

public class MysqlDbContext : DbContext
{
    public MysqlDbContext(DbContextOptions<MysqlDbContext> options) : base(options)
    {
    }

    public DbSet<MysqlEntity> MysqlEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(MysqlDbContext).Assembly);
        modelBuilder.Entity<MysqlEntity>().ToTable("test").HasKey(t => t.Id);
        base.OnModelCreating(modelBuilder);
    }
}