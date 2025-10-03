using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class BlogPostLikeInsertRequest
    {
        public Guid BlogPostId { get; set; }
        public string VisitorKey { get; set; } = string.Empty;
    }
}
