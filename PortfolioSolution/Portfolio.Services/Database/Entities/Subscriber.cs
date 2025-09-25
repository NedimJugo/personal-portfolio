using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class Subscriber
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Source { get; set; } // contact-form, blog, footer, etc.
        public DateTimeOffset SubscribedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UnsubscribedAt { get; set; }
    }
}
