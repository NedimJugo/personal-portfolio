using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class ProjectSearchObject : BaseSearchObject
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? ProjectType { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsPublished { get; set; }
    }
}
