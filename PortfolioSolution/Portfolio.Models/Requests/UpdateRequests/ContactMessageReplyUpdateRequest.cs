using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class ContactMessageReplyUpdateRequest
    {
        public string ReplyMessage { get; set; } = string.Empty;
        public string? ReplyToEmail { get; set; }
        public string? Subject { get; set; }
        public string DeliveryStatus { get; set; } = "sent";
        public DateTimeOffset? DeliveredAt { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsInternal { get; set; } = false;
    }
}
