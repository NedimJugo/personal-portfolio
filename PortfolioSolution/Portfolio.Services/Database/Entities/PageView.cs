using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class PageView
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Path { get; set; } = string.Empty;
        public string? Referrer { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public DateTimeOffset ViewedAt { get; set; } = DateTimeOffset.UtcNow;

        // For tracking specific content views
        public Guid? ProjectId { get; set; }
        public Guid? BlogPostId { get; set; }
        [MaxLength(100)]
        public string? VisitorKey { get; set; }
        // Navigation properties
        public virtual Project? Project { get; set; }
        public virtual BlogPost? BlogPost { get; set; }
    }
}
