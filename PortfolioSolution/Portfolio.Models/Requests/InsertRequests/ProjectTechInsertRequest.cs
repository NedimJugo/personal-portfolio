using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class ProjectTechInsertRequest
    {
        public Guid ProjectId { get; set; }
        public Guid TechId { get; set; }
    }
}
