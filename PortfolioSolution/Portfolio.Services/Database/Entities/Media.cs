using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class Media
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(256)]
        public string FileName { get; set; } = string.Empty;
        [MaxLength(256)]
        public string OriginalFileName { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string FileUrl { get; set; } = string.Empty;
        [MaxLength(50)]
        public string StorageProvider { get; set; } = "Local"; // Local, Azure, AWS
        [MaxLength(20)]
        public string FileType { get; set; } = "image"; // image, document, video
        public long FileSize { get; set; }
        [MaxLength(100)]
        public string MimeType { get; set; } = string.Empty;
        public int? Width { get; set; }
        public int? Height { get; set; }
        [MaxLength(256)]
        public string? AltText { get; set; }
        [MaxLength(512)]
        public string? Caption { get; set; }
        [MaxLength(100)]
        public string? Folder { get; set; }
        public int UploadedById { get; set; }
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public virtual ApplicationUser UploadedBy { get; set; } = null!;
        public virtual ICollection<Project> FeaturedProjects { get; set; } = new List<Project>();
        public virtual ICollection<ProjectImage> ProjectImages { get; set; } = new List<ProjectImage>();
    }


}
