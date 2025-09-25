using Portfolio.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class BlogPostResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImage { get; set; }
        public BlogPostStatus Status { get; set; }
        public string Tags { get; set; } = "[]";
        public string Categories { get; set; } = "[]";
        public int ReadingTime { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CreatedById { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
