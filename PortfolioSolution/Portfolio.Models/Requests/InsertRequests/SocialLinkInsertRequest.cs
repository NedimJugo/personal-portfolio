using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class SocialLinkInsertRequest
    {
        public string Platform { get; set; } = string.Empty; // github, linkedin, etc.
        public string DisplayName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string? Color { get; set; }
        public bool IsVisible { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
