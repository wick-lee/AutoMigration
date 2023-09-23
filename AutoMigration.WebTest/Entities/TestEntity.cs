using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMigration.WebTest.Entities;

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; }

    [Column(TypeName = "jsonb")] public Dictionary<Guid, ICollection<Guid>> Dictionary { get; set; }

    [Column(TypeName = "jsonb")] public TestEntityProperty Property { get; set; }

    public class TestEntityProperty
    {
        public string Text { get; set; }
    }
}