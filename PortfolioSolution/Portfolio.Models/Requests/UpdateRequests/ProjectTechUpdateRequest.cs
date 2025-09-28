using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class ProjectTechUpdateRequest
    {
        public Guid ProjectId { get; set; }
        public Guid TechId { get; set; }
    }
}
