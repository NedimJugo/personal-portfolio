using System.ComponentModel.DataAnnotations;

namespace Portfolio.Services.Database.Entities
{
    public class Tech
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        [MaxLength(20)]
        public string Category { get; set; } = "frontend"; // frontend, backend, database, tools, design

        public Guid? IconMediaId { get; set; }

        // Navigation properties
        public virtual Media? IconMedia { get; set; }
        public virtual ICollection<ProjectTech> ProjectTechs { get; set; } = new List<ProjectTech>();
    }
}
