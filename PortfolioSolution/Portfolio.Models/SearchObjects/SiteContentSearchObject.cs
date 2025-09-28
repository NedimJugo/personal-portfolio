using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class SiteContentSearchObject : BaseSearchObject
    {
        public string? Section { get; set; }
        public string? ContentType { get; set; }
        public bool? IsPublished { get; set; }
    }
}
