using Portfolio.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class ExperienceInsertRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public EmploymentType EmploymentType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public string Achievements { get; set; } = "[]";
        public string Technologies { get; set; } = "[]";
        public int DisplayOrder { get; set; } = 0;
    }
}
