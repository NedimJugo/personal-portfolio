using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class TechResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Category { get; set; } = "frontend";
        public Guid? IconMediaId { get; set; }

        // Navigation
        public MediaResponse? IconMedia { get; set; }
    }
}
