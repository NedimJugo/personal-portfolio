namespace PortfolioApi.Models.Entities
{
    public class ProjectImage
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int MediaId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public bool IsHero { get; set; } = false;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Media Media { get; set; } = null!;
    }
}
