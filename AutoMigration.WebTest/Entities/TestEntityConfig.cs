using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wick.AutoMigration;

namespace AutoMigration.WebTest.Entities;

public class TestEntityConfig : IEntityTypeConfiguration<TestEntity>
{
    public void Configure(EntityTypeBuilder<TestEntity> builder)
    {
        builder.ToTable("Test").HasKey(b => b.Id);
    }
}