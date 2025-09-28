using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class ProjectUpdateRequest
    {
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? ProjectType { get; set; }
        public string? Status { get; set; }
        public bool? IsFeatured { get; set; }
        public Guid? FeaturedMediaId { get; set; }
        public string? RepoUrl { get; set; }
        public string? LiveUrl { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool? IsPublished { get; set; }
        public int? DisplayOrder { get; set; }
        public int UpdatedById { get; set; }

        // Relations
        public List<ProjectImageUpdateRequest>? Images { get; set; }
        public List<Guid>? TagIds { get; set; }
        public List<Guid>? TechIds { get; set; }
    }
}
