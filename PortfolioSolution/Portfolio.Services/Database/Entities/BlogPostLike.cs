using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Database.Entities
{
    public class BlogPostLike
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BlogPostId { get; set; }
        public string VisitorKey { get; set; } = string.Empty;
        public DateTimeOffset LikedAt { get; set; } = DateTimeOffset.UtcNow;
        public virtual BlogPost BlogPost { get; set; } = null!;
    }
}
