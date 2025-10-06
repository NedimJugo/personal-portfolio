using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Requests.UpdateRequests
{
    public class CertificateUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTimeOffset IssueDate { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public string? CredentialId { get; set; }
        public string? CredentialUrl { get; set; }
        public string? Description { get; set; }
        public string? Skills { get; set; }
        public string CertificateType { get; set; } = "Certification";
        public bool IsActive { get; set; } = true;
        public bool IsPublished { get; set; } = true;
        public int DisplayOrder { get; set; }
        public Guid? LogoMediaId { get; set; }
        public Guid? CertificateMediaId { get; set; }
    }
}
