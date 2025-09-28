using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class PageViewSearchObject : BaseSearchObject
    {
        public string? Path { get; set; }
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? BlogPostId { get; set; }
    }
}
