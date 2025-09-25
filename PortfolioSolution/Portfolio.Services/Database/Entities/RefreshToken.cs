namespace Portfolio.Services.Database.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
