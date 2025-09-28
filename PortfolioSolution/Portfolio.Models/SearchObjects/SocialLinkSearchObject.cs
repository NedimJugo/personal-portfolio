using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class SocialLinkSearchObject : BaseSearchObject
    {
        public string? Platform { get; set; }
        public bool? IsVisible { get; set; }
    }
}
