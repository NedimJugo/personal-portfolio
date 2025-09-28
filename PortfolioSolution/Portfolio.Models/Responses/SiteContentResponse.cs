using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class SiteContentResponse
    {
        public Guid Id { get; set; }
        public string Section { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text";
        public string Content { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
