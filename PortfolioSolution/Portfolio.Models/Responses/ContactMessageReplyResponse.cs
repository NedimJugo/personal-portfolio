using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class ContactMessageReplyResponse
    {
        public Guid Id { get; set; }
        public Guid ContactMessageId { get; set; }
        public string ReplyMessage { get; set; } = string.Empty;
        public string? ReplyToEmail { get; set; }
        public string? Subject { get; set; }
        public int RepliedById { get; set; }
        public DateTimeOffset RepliedAt { get; set; }
        public string DeliveryStatus { get; set; } = string.Empty;
        public DateTimeOffset? DeliveredAt { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsInternal { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
