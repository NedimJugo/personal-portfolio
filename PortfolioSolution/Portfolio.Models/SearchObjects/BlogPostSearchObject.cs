using EcoChallenge.Models.SearchObjects;
using Portfolio.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class BlogPostSearchObject : BaseSearchObject
    {
        public string? Title { get; set; }
        public BlogPostStatus? Status { get; set; }
        public int? CreatedById { get; set; }
        public Guid? ProjectId { get; set; }
    }
}
