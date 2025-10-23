using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class EmailSendResult
    {
        public string Email { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
