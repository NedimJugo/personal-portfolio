using Portfolio.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class Testimonial : ISoftDeletable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(200)]
        public string ClientName { get; set; } = string.Empty;
        [MaxLength(200)]
        public string ClientTitle { get; set; } = string.Empty;
        [MaxLength(200)]
        public string ClientCompany { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? ClientAvatar { get; set; }
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
        [Range(1, 5)]
        public int Rating { get; set; } = 5;
        public bool IsApproved { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        // Link to specific project if relevant
        public Guid? ProjectId { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
        public virtual ApplicationUser? DeletedBy { get; set; }

        // Navigation properties
        public virtual Project? Project { get; set; }
    }
}
