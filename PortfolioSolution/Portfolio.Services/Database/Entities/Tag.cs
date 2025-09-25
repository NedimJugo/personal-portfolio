namespace Portfolio.Services.Database.Entities
{
    public class Tag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
    }
}
