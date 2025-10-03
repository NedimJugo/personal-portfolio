using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class BlogPostLikeSearchObject : BaseSearchObject
    {
        public Guid? BlogPostId { get; set; }
        public string? VisitorKey { get; set; }
    }
}
