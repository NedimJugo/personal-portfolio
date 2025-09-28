using EcoChallenge.Models.SearchObjects;
using Portfolio.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class ExperienceSearchObject : BaseSearchObject
    {
        public string? CompanyName { get; set; }
        public string? Position { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public bool? IsCurrent { get; set; }
    }
}
