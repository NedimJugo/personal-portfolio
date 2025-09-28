using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses.Auth
{
    public class RegisterResponse
    {
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
