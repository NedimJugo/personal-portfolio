using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class ProjectTagResponse
    {
        public Guid ProjectId { get; set; }
        public Guid TagId { get; set; }
    }
}
