using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class SocialLink
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)]
        public string Platform { get; set; } = string.Empty; // github, linkedin, twitter, etc.
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? IconClass { get; set; } // CSS icon class
        public string? Color { get; set; } // Brand color
        public bool IsVisible { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
