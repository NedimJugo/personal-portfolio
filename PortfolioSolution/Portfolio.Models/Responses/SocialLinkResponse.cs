using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class SocialLinkResponse
    {
        public Guid Id { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string? Color { get; set; }
        public bool IsVisible { get; set; }
        public int DisplayOrder { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
