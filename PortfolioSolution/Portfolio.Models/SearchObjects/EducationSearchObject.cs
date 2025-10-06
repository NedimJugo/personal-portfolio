using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class EducationSearchObject : BaseSearchObject
    {
        public string? InstitutionName { get; set; }
        public string? Degree { get; set; }
        public string? EducationType { get; set; }
        public bool? IsCurrent { get; set; }
    }
}
