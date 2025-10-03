using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class BlogPostLikeStatusResponse
    {
        public bool IsLiked { get; set; }
        public int TotalLikes { get; set; }
    }
}
