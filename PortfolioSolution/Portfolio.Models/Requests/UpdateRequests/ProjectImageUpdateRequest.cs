using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class ProjectImageUpdateRequest
    {
        public Guid ProjectId { get; set; }
        public Guid MediaId { get; set; }
        public string? Caption { get; set; }
        public int? Order { get; set; }
        public bool? IsHero { get; set; }
    }
}
