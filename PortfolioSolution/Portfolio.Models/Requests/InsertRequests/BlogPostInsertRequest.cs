using Portfolio.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class BlogPostInsertRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImage { get; set; }
        public BlogPostStatus Status { get; set; } = BlogPostStatus.Draft;
        public string Tags { get; set; } = "[]";
        public string Categories { get; set; } = "[]";
        public int CreatedById { get; set; }
        public Guid? ProjectId { get; set; }
    }

}
