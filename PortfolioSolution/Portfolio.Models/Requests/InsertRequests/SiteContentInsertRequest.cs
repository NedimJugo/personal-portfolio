using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class SiteContentInsertRequest
    {
        public string Section { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text"; // text, html, json
        public string Content { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
    }
}
