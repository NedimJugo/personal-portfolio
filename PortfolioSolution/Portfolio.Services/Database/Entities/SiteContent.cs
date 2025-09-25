using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class SiteContent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(100)]
        public string Section { get; set; } = string.Empty; // homepage_hero_title, about_intro, etc.
        [MaxLength(20)]
        public string ContentType { get; set; } = "text"; // text, html, json
        public string Content { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

}
