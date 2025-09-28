using Portfolio.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class ExperienceResponse
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public EmploymentType EmploymentType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Achievements { get; set; } = "[]";
        public string Technologies { get; set; } = "[]";
        public int DisplayOrder { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
