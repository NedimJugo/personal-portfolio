using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class TestimonialSearchObject : BaseSearchObject
    {
        public string? ClientName { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsFeatured { get; set; }
        public Guid? ProjectId { get; set; }
    }
}
