using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class SkillUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "frontend";
        public int ProficiencyLevel { get; set; } = 1;
        public int YearsExperience { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public Guid? IconMediaId { get; set; }
        public string? Color { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }

}
