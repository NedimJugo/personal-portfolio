namespace PortfolioApi.Models.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // Markdown/HTML
        public int? FeaturedMediaId { get; set; }
        public string? RepoUrl { get; set; }
        public string? LiveUrl { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTimeOffset? PublishedAt { get; set; }
        public int Order { get; set; } = 0;
        public int Views { get; set; } = 0;
        public int CreatedById { get; set; }
        public int UpdatedById { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public virtual Media? FeaturedMedia { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public virtual ApplicationUser UpdatedBy { get; set; } = null!;
        public virtual ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
        public virtual ICollection<ProjectTech> ProjectTechs { get; set; } = new List<ProjectTech>();
        public virtual ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
    }
}
