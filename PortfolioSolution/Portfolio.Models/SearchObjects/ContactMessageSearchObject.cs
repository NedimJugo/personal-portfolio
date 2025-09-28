using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class ContactMessageSearchObject : BaseSearchObject
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Subject { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public int? HandledById { get; set; }
    }
}
