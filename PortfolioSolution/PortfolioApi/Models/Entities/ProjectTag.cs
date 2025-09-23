namespace PortfolioApi.Models.Entities
{
    public class ProjectTag
    {
        public int ProjectId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
