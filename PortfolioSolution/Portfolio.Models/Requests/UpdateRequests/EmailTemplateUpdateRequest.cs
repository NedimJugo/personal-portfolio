using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class EmailTemplateUpdateRequest
    {
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public string? HtmlContent { get; set; }
        public string? TextContent { get; set; }
        public bool? IsActive { get; set; }
    }
}
