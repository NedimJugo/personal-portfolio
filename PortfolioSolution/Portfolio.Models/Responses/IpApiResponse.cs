using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class IpApiResponse
    {
        public string? Status { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }
}
