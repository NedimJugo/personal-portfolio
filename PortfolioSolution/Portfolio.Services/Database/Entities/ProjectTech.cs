namespace Portfolio.Services.Database.Entities
{
    public class ProjectTech
    {
        public Guid ProjectId { get; set; }
        public Guid TechId { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Tech Tech { get; set; } = null!;
    }
}
