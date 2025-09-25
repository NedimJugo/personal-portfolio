using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Portfolio.Services.Database.Entities;
using Portfolio.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Portfolio.Services.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            base.OnConfiguring(optionsBuilder);
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
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<SiteContent> SiteContents { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<PageView> PageViews { get; set; }
        public DbSet<SocialLink> SocialLinks { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

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
            ConfigureBlogPost(builder);
            ConfigureExperience(builder);
            ConfigureSiteContent(builder);
            ConfigureSkill(builder);
            ConfigurePageView(builder);
            ConfigureSocialLink(builder);
            ConfigureTestimonial(builder);
            ConfigureSubscriber(builder);
            ConfigureEmailTemplate(builder);

            // Apply soft delete query filters
            ApplySoftDeleteQueryFilters(builder);

            // Seed data
            builder.SeedData();
        }

        private void ApplySoftDeleteQueryFilters(ModelBuilder builder)
        {
            // Apply global query filters for soft delete
            builder.Entity<BlogPost>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<ContactMessage>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Experience>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Testimonial>().HasQueryFilter(e => !e.IsDeleted);

            // Add filters for entities that depend on Project
            builder.Entity<ProjectImage>().HasQueryFilter(e => !e.Project.IsDeleted);
            builder.Entity<ProjectTag>().HasQueryFilter(e => !e.Project.IsDeleted);
            builder.Entity<ProjectTech>().HasQueryFilter(e => !e.Project.IsDeleted);
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

                entity.Property(e => e.FileUrl)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(e => e.StorageProvider)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.FileType)
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

                // Soft delete relationship
                entity.HasOne(e => e.DeletedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.IsPublished, e.PublishedAt });
                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.IsDeleted);
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

                // Soft delete relationship
                entity.HasOne(e => e.DeletedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsDeleted);
            });
        }

        private void ConfigureBlogPost(ModelBuilder builder)
        {
            builder.Entity<BlogPost>(entity =>
            {
                entity.Property(e => e.Title)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.Slug)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.HasIndex(e => e.Slug)
                    .IsUnique();

                entity.Property(e => e.Excerpt)
                    .HasMaxLength(512);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                // Soft delete relationship
                entity.HasOne(e => e.DeletedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.Status, e.PublishedAt });
                entity.HasIndex(e => e.IsDeleted);
            });
        }

        private void ConfigureExperience(ModelBuilder builder)
        {
            builder.Entity<Experience>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(36); // For GUID string

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Position)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Location)
                    .HasMaxLength(200);

                // Soft delete relationship
                entity.HasOne(e => e.DeletedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.IsDeleted);
            });
        }

        private void ConfigureTestimonial(ModelBuilder builder)
        {
            builder.Entity<Testimonial>(entity =>
            {
                entity.Property(e => e.ClientName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.ClientTitle)
                    .HasMaxLength(200);

                entity.Property(e => e.ClientCompany)
                    .HasMaxLength(200);

                entity.Property(e => e.Content)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Soft delete relationship
                entity.HasOne(e => e.DeletedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.IsDeleted);
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

        private void ConfigureSiteContent(ModelBuilder builder)
        {
            builder.Entity<SiteContent>(entity =>
            {
                entity.Property(e => e.Section)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.ContentType)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(e => e.Section);
                entity.HasIndex(e => e.IsPublished);
            });
        }

        private void ConfigureSkill(ModelBuilder builder)
        {
            builder.Entity<Skill>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Category)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasOne(e => e.IconMedia)
                    .WithMany()
                    .HasForeignKey(e => e.IconMediaId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.DisplayOrder);
            });
        }

        private void ConfigurePageView(ModelBuilder builder)
        {
            builder.Entity<PageView>(entity =>
            {
                entity.Property(e => e.Path)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.BlogPost)
                    .WithMany()
                    .HasForeignKey(e => e.BlogPostId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.ViewedAt);
                entity.HasIndex(e => e.Path);
            });
        }

        private void ConfigureSocialLink(ModelBuilder builder)
        {
            builder.Entity<SocialLink>(entity =>
            {
                entity.Property(e => e.Platform)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Url)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.IsVisible);
            });
        }

        private void ConfigureSubscriber(ModelBuilder builder)
        {
            builder.Entity<Subscriber>(entity =>
            {
                entity.Property(e => e.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.Name)
                    .HasMaxLength(200);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.HasIndex(e => e.IsActive);
            });
        }

        private void ConfigureEmailTemplate(ModelBuilder builder)
        {
            builder.Entity<EmailTemplate>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Subject)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.HasIndex(e => e.IsActive);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            HandleSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            HandleSoftDelete();
            return base.SaveChanges();
        }

        private void HandleSoftDelete()
        {
            var deletedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDeletable);

            foreach (var entry in deletedEntries)
            {
                entry.State = EntityState.Modified;
                var entity = (ISoftDeletable)entry.Entity;
                entity.IsDeleted = true;
                entity.DeletedAt = DateTimeOffset.UtcNow;
                // Note: You'll need to set DeletedById from your current user context
                // This can be done through dependency injection or a service
            }
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Skip timestamp updates during seeding (when entities are in Added state but have pre-set timestamps)
                if (entry.State == EntityState.Added)
                {
                    var entityType = entry.Entity.GetType();
                    var createdAtProperty = entityType.GetProperty("CreatedAt");
                    var updatedAtProperty = entityType.GetProperty("UpdatedAt");

                    // If CreatedAt is already set to a specific date (not default), this is likely seeded data
                    if (createdAtProperty?.GetValue(entry.Entity) is DateTimeOffset createdAt &&
                        createdAt != default(DateTimeOffset))
                    {
                        // This is seeded data, don't update timestamps
                        continue;
                    }
                }

                // Handle entities with UpdatedAt property
                var entityType2 = entry.Entity.GetType();
                var updatedAtProperty2 = entityType2.GetProperty("UpdatedAt");

                if (updatedAtProperty2 != null && updatedAtProperty2.PropertyType == typeof(DateTimeOffset))
                {
                    if (entry.State == EntityState.Modified)
                    {
                        updatedAtProperty2.SetValue(entry.Entity, DateTimeOffset.UtcNow);
                    }
                }
            }
        }
    }
}