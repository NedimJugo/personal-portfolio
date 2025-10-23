using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.InsertRequests
{
    public class ContactMessageReplyInsertRequest
    {
        public Guid ContactMessageId { get; set; }
        public string ReplyMessage { get; set; } = string.Empty;
        public string? ReplyToEmail { get; set; }
        public string? Subject { get; set; }
        public bool IsInternal { get; set; } = false;
    }
}
