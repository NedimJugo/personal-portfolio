using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class ProjectImageResponse
    {
        public Guid Id { get; set; }
        public Guid MediaId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsHero { get; set; }
    }
}
