using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Database.Entities
{
    public class ContactMessageReply : ISoftDeletable
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ContactMessageId { get; set; }

        [MaxLength(5000)]
        public string ReplyMessage { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? ReplyToEmail { get; set; } // Can override the original email

        [MaxLength(300)]
        public string? Subject { get; set; }

        public int RepliedById { get; set; }

        public DateTimeOffset RepliedAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(20)]
        public string DeliveryStatus { get; set; } = "sent";

        public DateTimeOffset? DeliveredAt { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public bool IsInternal { get; set; } = false;
        public string? ExternalMessageId { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedById { get; set; }

        // Navigation properties
        public virtual ContactMessage ContactMessage { get; set; } = null!;
        public virtual ApplicationUser RepliedBy { get; set; } = null!;
        public virtual ApplicationUser? DeletedBy { get; set; }
    }
}
