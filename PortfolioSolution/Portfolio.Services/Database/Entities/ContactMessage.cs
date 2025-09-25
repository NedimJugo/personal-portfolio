using Portfolio.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class ContactMessage : ISoftDeletable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(300)]
        public string Subject { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? Phone { get; set; }
        [MaxLength(200)]
        public string? Company { get; set; }
        [MaxLength(100)]
        public string? ProjectType { get; set; }
        [MaxLength(50)]
        public string? BudgetRange { get; set; }
        [MaxLength(100)]
        public string? IpAddress { get; set; }
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        public string? Source { get; set; } // website, linkedin, referral, etc.
        [MaxLength(20)]
        public string Status { get; set; } = "new"; // new, read, replied, archived
        [MaxLength(20)]
        public string Priority { get; set; } = "medium"; // low, medium, high
        public int? HandledById { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
        public virtual ApplicationUser? DeletedBy { get; set; }

        // Navigation properties
        public virtual ApplicationUser? HandledBy { get; set; }
    }

}
