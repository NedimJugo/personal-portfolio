using Microsoft.AspNetCore.Identity;

namespace PortfolioApi.Models.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
        public virtual ICollection<Project> UpdatedProjects { get; set; } = new List<Project>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
