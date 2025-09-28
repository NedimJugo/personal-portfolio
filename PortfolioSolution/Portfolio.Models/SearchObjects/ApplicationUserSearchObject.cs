using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class ApplicationUserSearchObject : BaseSearchObject
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
    }
}
