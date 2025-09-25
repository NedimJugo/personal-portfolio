using Portfolio.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class BlogPostUpdateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImage { get; set; }
        public BlogPostStatus Status { get; set; } = BlogPostStatus.Draft;
        public string Tags { get; set; } = "[]";
        public string Categories { get; set; } = "[]";
        public Guid? ProjectId { get; set; }
    }
}
