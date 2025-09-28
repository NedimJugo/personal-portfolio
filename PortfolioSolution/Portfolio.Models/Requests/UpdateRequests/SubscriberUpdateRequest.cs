using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class SubscriberUpdateRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Source { get; set; }
        public DateTimeOffset? UnsubscribedAt { get; set; }
    }
}
