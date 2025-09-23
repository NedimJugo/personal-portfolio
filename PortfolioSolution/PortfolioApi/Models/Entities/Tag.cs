namespace PortfolioApi.Models.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
    }
}
