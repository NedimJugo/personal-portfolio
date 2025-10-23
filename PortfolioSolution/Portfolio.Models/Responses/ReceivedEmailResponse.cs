using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class ReceivedEmailResponse
    {
        public string MessageId { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ReplyTo { get; set; }
        public DateTimeOffset ReceivedDate { get; set; }
        public bool IsReply { get; set; }
        public string? InReplyTo { get; set; }
    }
}
