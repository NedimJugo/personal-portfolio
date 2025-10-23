using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class ContactMessageReplySearchObject : BaseSearchObject
    {
        public Guid? ContactMessageId { get; set; }
        public int? RepliedById { get; set; }
        public string? DeliveryStatus { get; set; }
        public bool? IsInternal { get; set; }
        public DateTimeOffset? RepliedFrom { get; set; }
        public DateTimeOffset? RepliedTo { get; set; }
    }
}
