using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class ContactMessageInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public string? ProjectType { get; set; }
        public string? BudgetRange { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Source { get; set; }
        public string Status { get; set; } = "new";
        public string Priority { get; set; } = "medium";
        public int? HandledById { get; set; }

        /// <summary>
        /// Honeypot anti-spam field. Left blank by real users (hidden via CSS),
        /// filled in by naive bots. Any non-empty value causes the submission to be silently dropped.
        /// </summary>
        public string? Website { get; set; }
    }
}
