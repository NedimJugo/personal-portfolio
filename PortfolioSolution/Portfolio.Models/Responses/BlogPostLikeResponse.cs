using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class BlogPostLikeResponse
    {
        public Guid Id { get; set; }
        public Guid BlogPostId { get; set; }
        public string VisitorKey { get; set; } = string.Empty;
        public DateTimeOffset LikedAt { get; set; }
    }
}
