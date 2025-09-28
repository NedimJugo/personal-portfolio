using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class SocialLinkUpdateRequest
    {
        public string Platform { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string? Color { get; set; }
        public bool IsVisible { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}
