using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class TagSearchObject : BaseSearchObject
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
    }
}
