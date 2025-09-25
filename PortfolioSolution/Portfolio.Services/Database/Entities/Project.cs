using Portfolio.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class Project : ISoftDeletable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(256)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(256)]
        public string Slug { get; set; } = string.Empty;
        [MaxLength(512)]
        public string ShortDescription { get; set; } = string.Empty;
        public string FullDescription { get; set; } = string.Empty; // Markdown/HTML content
        [MaxLength(20)]
        public string ProjectType { get; set; } = "web"; // web, mobile, desktop, api
        [MaxLength(20)]
        public string Status { get; set; } = "completed"; // completed, ongoing, archived
        public bool IsFeatured { get; set; } = false;
        public Guid? FeaturedMediaId { get; set; }
        [MaxLength(1000)]
        public string? RepoUrl { get; set; }
        [MaxLength(1000)]
        public string? LiveUrl { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTimeOffset? PublishedAt { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public int ViewCount { get; set; } = 0;
        public int CreatedById { get; set; }
        public int UpdatedById { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
        public virtual ApplicationUser? DeletedBy { get; set; }

        // Navigation properties
        public virtual Media? FeaturedMedia { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public virtual ApplicationUser UpdatedBy { get; set; } = null!;
        public virtual ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
        public virtual ICollection<ProjectTech> ProjectTechs { get; set; } = new List<ProjectTech>();
        public virtual ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
    }

}
