using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class PageViewInsertRequest
    {
        public string Path { get; set; } = string.Empty;
        public string? Referrer { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? VisitorKey { get; set; } // Add this
        public Guid? ProjectId { get; set; }
        public Guid? BlogPostId { get; set; }
    }
}
