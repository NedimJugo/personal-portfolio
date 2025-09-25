using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class EmailTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // contact-response, project-inquiry, etc.
        [MaxLength(256)]
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
