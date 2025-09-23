namespace PortfolioApi.Models.Entities
{
    public class ProjectTech
    {
        public int ProjectId { get; set; }
        public int TechId { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Tech Tech { get; set; } = null!;
    }
}
