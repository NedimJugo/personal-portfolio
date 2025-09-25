using Portfolio.Services.Database.Entities;

namespace Portfolio.Services.Interfaces
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTimeOffset? DeletedAt { get; set; }
        int? DeletedById { get; set; }
        ApplicationUser? DeletedBy { get; set; }
    }

}
