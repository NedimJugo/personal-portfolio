namespace PortfolioApi.Models.Entities
{
    public class Settings
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
