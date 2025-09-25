namespace Portfolio.Services.Database.Entities
{
    public class ProjectImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public Guid MediaId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public bool IsHero { get; set; } = false;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Media Media { get; set; } = null!;
    }
}
