using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using PortfolioApi.Models.Entities;

namespace PortfolioApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectImage> ProjectImages { get; set; }
        public DbSet<Tech> Techs { get; set; }
        public DbSet<ProjectTech> ProjectTechs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProjectTag> ProjectTags { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity table names
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

            // Configure entities
            ConfigureUser(builder);
            ConfigureRefreshToken(builder);
            ConfigureMedia(builder);
            ConfigureProject(builder);
            ConfigureProjectImage(builder);
            ConfigureTech(builder);
            ConfigureProjectTech(builder);
            ConfigureTag(builder);
            ConfigureProjectTag(builder);
            ConfigureContactMessage(builder);
            ConfigureSettings(builder);
        }

        private void ConfigureUser(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });
        }

        private void ConfigureRefreshToken(ModelBuilder builder)
        {
            builder.Entity<RefreshToken>(entity =>
            {
                entity.Property(e => e.Token)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.HasIndex(e => e.Token)
                    .IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureMedia(ModelBuilder builder)
        {
            builder.Entity<Media>(entity =>
            {
                entity.Property(e => e.FileName)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.Url)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(e => e.StorageProvider)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.ContentType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasOne(e => e.UploadedBy)
                    .WithMany()
                    .HasForeignKey(e => e.UploadedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureProject(ModelBuilder builder)
        {
            builder.Entity<Project>(entity =>
            {
                entity.Property(e => e.Title)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.Slug)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.HasIndex(e => e.Slug)
                    .IsUnique();

                entity.Property(e => e.ShortDescription)
                    .HasMaxLength(512)
                    .IsRequired();

                entity.Property(e => e.RepoUrl)
                    .HasMaxLength(1000);

                entity.Property(e => e.LiveUrl)
                    .HasMaxLength(1000);

                entity.HasOne(e => e.FeaturedMedia)
                    .WithMany(m => m.FeaturedProjects)
                    .HasForeignKey(e => e.FeaturedMediaId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.CreatedProjects)
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UpdatedBy)
                    .WithMany(u => u.UpdatedProjects)
                    .HasForeignKey(e => e.UpdatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.IsPublished, e.PublishedAt });
                entity.HasIndex(e => e.Order);
            });
        }

        private void ConfigureProjectImage(ModelBuilder builder)
        {
            builder.Entity<ProjectImage>(entity =>
            {
                entity.Property(e => e.Caption)
                    .HasMaxLength(256);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.Images)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Media)
                    .WithMany(m => m.ProjectImages)
                    .HasForeignKey(e => e.MediaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ProjectId, e.Order });
            });
        }

        private void ConfigureTech(ModelBuilder builder)
        {
            builder.Entity<Tech>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Slug)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(e => e.Slug)
                    .IsUnique();

                entity.HasOne(e => e.IconMedia)
                    .WithMany()
                    .HasForeignKey(e => e.IconMediaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigureProjectTech(ModelBuilder builder)
        {
            builder.Entity<ProjectTech>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.TechId });

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.ProjectTechs)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tech)
                    .WithMany(t => t.ProjectTechs)
                    .HasForeignKey(e => e.TechId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureTag(ModelBuilder builder)
        {
            builder.Entity<Tag>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Slug)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(e => e.Slug)
                    .IsUnique();
            });
        }

        private void ConfigureProjectTag(ModelBuilder builder)
        {
            builder.Entity<ProjectTag>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.TagId });

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.ProjectTags)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.ProjectTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureContactMessage(ModelBuilder builder)
        {
            builder.Entity<ContactMessage>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.Subject)
                    .HasMaxLength(300)
                    .IsRequired();

                entity.Property(e => e.Message)
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.HasOne(e => e.HandledBy)
                    .WithMany()
                    .HasForeignKey(e => e.HandledById)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });
        }

        private void ConfigureSettings(ModelBuilder builder)
        {
            builder.Entity<Settings>(entity =>
            {
                entity.HasKey(e => e.Key);

                entity.Property(e => e.Key)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Value)
                    .HasMaxLength(4000)
                    .IsRequired();
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Project project)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        project.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                }
            }
        }
    }
}
