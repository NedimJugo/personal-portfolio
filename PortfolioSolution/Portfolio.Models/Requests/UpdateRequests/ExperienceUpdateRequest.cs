using Portfolio.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class ExperienceUpdateRequest
    {
        public string? CompanyName { get; set; }
        public string? CompanyLogo { get; set; }
        public string? Position { get; set; }
        public string? Location { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool? IsCurrent { get; set; }
        public string? Description { get; set; }
        public string? Achievements { get; set; }
        public string? Technologies { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
