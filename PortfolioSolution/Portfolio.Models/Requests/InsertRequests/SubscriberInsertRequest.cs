using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class SubscriberInsertRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Source { get; set; } // contact-form, blog, footer, etc.

        /// <summary>
        /// Honeypot anti-spam field. Left blank by real users (hidden via CSS),
        /// filled in by naive bots. Any non-empty value causes the submission to be silently dropped.
        /// </summary>
        public string? Website { get; set; }
    }
}
