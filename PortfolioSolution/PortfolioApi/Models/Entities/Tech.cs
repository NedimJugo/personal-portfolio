namespace PortfolioApi.Models.Entities
{
    public class Tech
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? IconMediaId { get; set; }

        // Navigation properties
        public virtual Media? IconMedia { get; set; }
        public virtual ICollection<ProjectTech> ProjectTechs { get; set; } = new List<ProjectTech>();
    }
}
