using PortfolioApi.Models.Entities;

namespace PortfolioApi.Models.DTOs
{
    public class ContactMessageResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ContactStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int? HandledById { get; set; }
    }
}
