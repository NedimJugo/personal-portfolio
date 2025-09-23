namespace PortfolioApi.Models.Entities
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ContactStatus Status { get; set; } = ContactStatus.New;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int? HandledById { get; set; }

        // Navigation properties
        public virtual ApplicationUser? HandledBy { get; set; }
    }

    public enum ContactStatus
    {
        New = 0,
        Read = 1,
        Closed = 2
    }
}
