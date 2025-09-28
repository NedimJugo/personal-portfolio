using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class TestimonialInsertRequest
    {
        public string ClientName { get; set; } = string.Empty;
        public string ClientTitle { get; set; } = string.Empty;
        public string ClientCompany { get; set; } = string.Empty;
        public string? ClientAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public bool IsApproved { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public Guid? ProjectId { get; set; }
    }
}
