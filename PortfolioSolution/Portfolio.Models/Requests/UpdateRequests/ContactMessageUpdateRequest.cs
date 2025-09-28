using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class ContactMessageUpdateRequest
    {
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public string? ProjectType { get; set; }
        public string? BudgetRange { get; set; }
        public string? Source { get; set; }
        public string Status { get; set; } = "new";
        public string Priority { get; set; } = "medium";
        public int? HandledById { get; set; }
    }
}
