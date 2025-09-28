using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class SkillSearchObject : BaseSearchObject
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public bool? IsFeatured { get; set; }
        public int? MinProficiency { get; set; }
        public int? MaxProficiency { get; set; }
    }
}
