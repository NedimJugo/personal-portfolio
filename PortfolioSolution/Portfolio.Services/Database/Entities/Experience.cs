using Portfolio.Services.Interfaces;

namespace Portfolio.Services.Database.Entities
{
    public class Experience : ISoftDeletable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public EmploymentType EmploymentType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; } // Null for current job
        public bool IsCurrent { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public string Achievements { get; set; } = "[]"; // JSON array as string
        public string Technologies { get; set; } = "[]"; // JSON array as string
        public int DisplayOrder { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
        public virtual ApplicationUser? DeletedBy { get; set; }
    }
    public enum EmploymentType
    {
        FullTime = 0,
        PartTime = 1,
        Contract = 2,
        Internship = 3
    }
}
