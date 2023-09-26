using AutoMigration.WebTest.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoMigration.WebTest;

public class NpgsqlDbContext : DbContext
{
    public NpgsqlDbContext(DbContextOptions<NpgsqlDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>().ToTable("test").HasKey(t => t.Id);
        base.OnModelCreating(modelBuilder);
    }
}