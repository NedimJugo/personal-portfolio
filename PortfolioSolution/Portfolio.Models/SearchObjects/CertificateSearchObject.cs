using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class CertificateSearchObject : BaseSearchObject
    {
        public string? Name { get; set; }
        public string? IssuingOrganization { get; set; }
        public string? CertificateType { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPublished { get; set; }
    }
}
