namespace PortfolioApi.Models.Entities
{
    public class Media
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string StorageProvider { get; set; } = "Local"; // Local, Azure, AWS
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int UploadedById { get; set; }
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public virtual ApplicationUser UploadedBy { get; set; } = null!;
        public virtual ICollection<Project> FeaturedProjects { get; set; } = new List<Project>();
        public virtual ICollection<ProjectImage> ProjectImages { get; set; } = new List<ProjectImage>();
    }
}
