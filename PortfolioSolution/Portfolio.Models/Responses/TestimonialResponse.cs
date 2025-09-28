using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class TestimonialResponse
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientTitle { get; set; } = string.Empty;
        public string ClientCompany { get; set; } = string.Empty;
        public string? ClientAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? ProjectId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Soft delete metadata
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedById { get; set; }

        // Navigation
        public ProjectResponse? Project { get; set; }
    }
}
