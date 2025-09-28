using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class ProjectImageInsertRequest
    {
        public Guid MediaId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public bool IsHero { get; set; } = false;
    }
}
