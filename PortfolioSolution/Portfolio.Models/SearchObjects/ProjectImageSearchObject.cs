using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class ProjectImageSearchObject : BaseSearchObject
    {
        public Guid? ProjectId { get; set; }
        public Guid? MediaId { get; set; }
        public bool? IsHero { get; set; }
    }
}
