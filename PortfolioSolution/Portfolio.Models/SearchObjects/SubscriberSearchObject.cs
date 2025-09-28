using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class SubscriberSearchObject : BaseSearchObject
    {
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public string? Source { get; set; }
    }
}
