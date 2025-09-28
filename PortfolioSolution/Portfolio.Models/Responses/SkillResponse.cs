using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class SkillResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "frontend";
        public int ProficiencyLevel { get; set; }
        public int YearsExperience { get; set; }
        public bool IsFeatured { get; set; }
        public Guid? IconMediaId { get; set; }
        public string? Color { get; set; }
        public int DisplayOrder { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation
        public MediaResponse? IconMedia { get; set; }
    }
}
