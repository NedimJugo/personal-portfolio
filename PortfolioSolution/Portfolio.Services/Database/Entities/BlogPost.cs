using Portfolio.Models.Enums;
using Portfolio.Services.Interfaces;

namespace Portfolio.Services.Database.Entities
{
    public class BlogPost : ISoftDeletable
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Keep as Guid for new entities
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImage { get; set; }
        public BlogPostStatus Status { get; set; } = BlogPostStatus.Draft;
        public string Tags { get; set; } = "[]";
        public string Categories { get; set; } = "[]";
        public int ReadingTime { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public Guid? ProjectId { get; set; }
        // FIX: Should be int to match ApplicationUser.Id
        public int CreatedById { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? PublishedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
        public virtual ApplicationUser? DeletedBy { get; set; }

        // Navigation properties
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        // Navigation properties
        public virtual Project? Project { get; set; }
    }

}
