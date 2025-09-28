using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class ProjectResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string FullDescription { get; set; } = string.Empty;
        public string ProjectType { get; set; } = "web";
        public string Status { get; set; } = "completed";
        public bool IsFeatured { get; set; }
        public Guid? FeaturedMediaId { get; set; }
        public string? RepoUrl { get; set; }
        public string? LiveUrl { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsPublished { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public int DisplayOrder { get; set; }
        public int ViewCount { get; set; }
        public int CreatedById { get; set; }
        public int UpdatedById { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Relations
        public List<ProjectImageResponse> Images { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
        public List<Guid> TechIds { get; set; } = new();
    }
}
