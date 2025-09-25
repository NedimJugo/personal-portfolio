using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class Skill
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(20)]
        public string Category { get; set; } = "frontend";
        [Range(1, 5)]
        public int ProficiencyLevel { get; set; } = 1;
        [Range(0, 50)]
        public int YearsExperience { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public Guid? IconMediaId { get; set; }

        [MaxLength(7)]
        public string? Color { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public virtual Media? IconMedia { get; set; }
    }

}
