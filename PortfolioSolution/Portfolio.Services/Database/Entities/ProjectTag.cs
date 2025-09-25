namespace Portfolio.Services.Database.Entities
{
    public class ProjectTag
    {
        public Guid ProjectId { get; set; }
        public Guid TagId { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
