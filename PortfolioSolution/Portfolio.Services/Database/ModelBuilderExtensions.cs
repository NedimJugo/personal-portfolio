using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Models.Enums;
using Portfolio.Services.Database.Entities;

namespace Portfolio.Services.Database
{
    public static class ModelBuilderExtensions
    {
        // Static values to replace dynamic ones
        private static readonly DateTimeOffset SeedCreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset ProjectCreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset ProjectPublishedAt = new DateTimeOffset(2024, 8, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateOnly ProjectStartDate = new DateOnly(2024, 3, 1);
        private static readonly DateOnly ProjectEndDate = new DateOnly(2024, 7, 31);
        private static readonly DateOnly ExperienceStartDate = new DateOnly(2021, 1, 1);
        private static readonly DateOnly ExperienceEndDate = new DateOnly(2024, 1, 1);

        // Additional static values for the second project
        private static readonly DateTimeOffset Project2PublishedAt = new DateTimeOffset(2024, 8, 15, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset Project2CreatedAt = new DateTimeOffset(2024, 8, 15, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateOnly Project2StartDate = new DateOnly(2024, 5, 1);
        private static readonly DateOnly Experience2StartDate = new DateOnly(2019, 1, 1);

        // Static GUID strings for consistent seeding
        private static readonly string AdminSecurityStamp = "550e8400-e29b-41d4-a716-446655440100";
        private static readonly string AdminConcurrencyStamp = "550e8400-e29b-41d4-a716-446655440101";

        private static readonly DateTimeOffset SeedUpdatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static void SeedData(this ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedAdminUser(modelBuilder);
            SeedUserRoles(modelBuilder);
            SeedTechnologies(modelBuilder);
            SeedTags(modelBuilder);
            SeedSkills(modelBuilder);
            SeedProjects(modelBuilder);
            SeedExperiences(modelBuilder);
            SeedTestimonials(modelBuilder);
            SeedSocialLinks(modelBuilder);
            SeedSiteContent(modelBuilder);
            SeedEmailTemplates(modelBuilder);
            SeedSettings(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int>
                {
                    Id = 1,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "550e8400-e29b-41d4-a716-446655440001"
                },
                new IdentityRole<int>
                {
                    Id = 2,
                    Name = "Editor",
                    NormalizedName = "EDITOR",
                    ConcurrencyStamp = "550e8400-e29b-41d4-a716-446655440002"
                },
                new IdentityRole<int>
                {
                    Id = 3,
                    Name = "Viewer",
                    NormalizedName = "VIEWER",
                    ConcurrencyStamp = "550e8400-e29b-41d4-a716-446655440003"
                }
            );
        }

        private static void SeedAdminUser(ModelBuilder modelBuilder)
        {
            // Use a pre-generated static password hash for "Admin123!"
            const string staticPasswordHash = "AQAAAAIAAYagAAAAEKlnP9oP3//IHdit99LuXDtM9FCWulHQ/LCf01Ip1NK7MOf3kI6RQwuv66sIUPtl4w==";

            var adminUser = new ApplicationUser
            {
                Id = 1,
                UserName = "admin@portfolio.com",
                NormalizedUserName = "ADMIN@PORTFOLIO.COM",
                Email = "admin@portfolio.com",
                NormalizedEmail = "ADMIN@PORTFOLIO.COM",
                EmailConfirmed = true,
                FullName = "Portfolio Admin",
                IsActive = true,
                SecurityStamp = AdminSecurityStamp,
                ConcurrencyStamp = AdminConcurrencyStamp,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedUpdatedAt,
                PasswordHash = staticPasswordHash
            };

            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);
        }

        private static void SeedUserRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int>
                {
                    UserId = 1,
                    RoleId = 1
                }
            );
        }

        private static void SeedTechnologies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tech>().HasData(
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Angular", Slug = "angular", Category = "frontend" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "React", Slug = "react", Category = "frontend" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "Vue.js", Slug = "vuejs", Category = "frontend" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = "ASP.NET Core", Slug = "aspnet-core", Category = "backend" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Name = "Node.js", Slug = "nodejs", Category = "backend" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000006"), Name = "TypeScript", Slug = "typescript", Category = "frontend" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000007"), Name = "C#", Slug = "csharp", Category = "backend" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000008"), Name = "Python", Slug = "python", Category = "backend" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000009"), Name = "Entity Framework", Slug = "entity-framework", Category = "database" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000010"), Name = "SQL Server", Slug = "sql-server", Category = "database" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000011"), Name = "PostgreSQL", Slug = "postgresql", Category = "database" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000012"), Name = "MongoDB", Slug = "mongodb", Category = "database" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000013"), Name = "Azure", Slug = "azure", Category = "tools" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000014"), Name = "AWS", Slug = "aws", Category = "tools" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000015"), Name = "Docker", Slug = "docker", Category = "tools" },
                new Tech { Id = Guid.Parse("00000000-0000-0000-0000-000000000016"), Name = "Git", Slug = "git", Category = "tools" }
            );
        }

        private static void SeedTags(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>().HasData(
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Web Development", Slug = "web-development" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Full Stack", Slug = "full-stack" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "API", Slug = "api" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = "SPA", Slug = "spa" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Name = "Cloud", Slug = "cloud" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000006"), Name = "Mobile", Slug = "mobile" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000007"), Name = "Desktop", Slug = "desktop" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000008"), Name = "E-commerce", Slug = "ecommerce" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000009"), Name = "CMS", Slug = "cms" },
                new Tag { Id = Guid.Parse("00000000-0000-0000-0000-000000000010"), Name = "Dashboard", Slug = "dashboard" }
            );
        }

        private static void SeedSkills(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Skill>().HasData(
                new Skill
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Name = "C#",
                    Category = "backend",
                    ProficiencyLevel = 5,
                    YearsExperience = 8,
                    IsFeatured = true,
                    DisplayOrder = 1,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt,
                },
                new Skill
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Name = "ASP.NET Core",
                    Category = "backend",
                    ProficiencyLevel = 5,
                    YearsExperience = 6,
                    IsFeatured = true,
                    DisplayOrder = 2,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Skill
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Name = "Angular",
                    Category = "frontend",
                    ProficiencyLevel = 4,
                    YearsExperience = 5,
                    IsFeatured = true,
                    DisplayOrder = 3,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Skill
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    Name = "TypeScript",
                    Category = "frontend",
                    ProficiencyLevel = 4,
                    YearsExperience = 4,
                    IsFeatured = true,
                    DisplayOrder = 4,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Skill
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    Name = "SQL Server",
                    Category = "database",
                    ProficiencyLevel = 4,
                    YearsExperience = 7,
                    IsFeatured = true,
                    DisplayOrder = 5,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Skill
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                    Name = "Entity Framework",
                    Category = "backend",
                    ProficiencyLevel = 5,
                    YearsExperience = 6,
                    IsFeatured = false,
                    DisplayOrder = 6,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Skill
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
                    Name = "Azure",
                    Category = "cloud",
                    ProficiencyLevel = 3,
                    YearsExperience = 3,
                    IsFeatured = false,
                    DisplayOrder = 7,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Skill
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
                    Name = "Docker",
                    Category = "tools",
                    ProficiencyLevel = 3,
                    YearsExperience = 2,
                    IsFeatured = false,
                    DisplayOrder = 8,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                }
            );
        }

        private static void SeedProjects(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasData(
                new Project
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Title = "Portfolio Management System",
                    Slug = "portfolio-management-system",
                    ShortDescription = "A comprehensive portfolio management API built with ASP.NET Core",
                    FullDescription = "# Portfolio Management System\n\nA full-featured portfolio management system with user authentication, project management, blog functionality, and more.\n\n## Features\n- User authentication and authorization\n- Project portfolio management\n- Blog system\n- Contact form handling\n- Media management\n- Analytics and tracking",
                    ProjectType = "api",
                    Status = "completed",
                    IsFeatured = true,
                    IsPublished = true,
                    PublishedAt = ProjectPublishedAt,
                    StartDate = ProjectStartDate,
                    EndDate = ProjectEndDate,
                    CreatedById = 1,
                    UpdatedById = 1,
                    DisplayOrder = 1,
                    CreatedAt = ProjectCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Project
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Title = "E-commerce Angular Application",
                    Slug = "ecommerce-angular-app",
                    ShortDescription = "Modern e-commerce frontend built with Angular and TypeScript",
                    FullDescription = "# E-commerce Application\n\nA modern, responsive e-commerce application built with Angular.\n\n## Features\n- Product catalog\n- Shopping cart\n- User authentication\n- Payment integration\n- Order management",
                    ProjectType = "web",
                    Status = "ongoing",
                    IsFeatured = true,
                    IsPublished = true,
                    PublishedAt = Project2PublishedAt,
                    StartDate = Project2StartDate,
                    CreatedById = 1,
                    UpdatedById = 1,
                    DisplayOrder = 2,
                    CreatedAt = Project2CreatedAt,
                    UpdatedAt = SeedUpdatedAt

                }
            );
        }

        private static void SeedExperiences(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Experience>().HasData(
                new Experience
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    CompanyName = "Tech Solutions Inc.",
                    Position = "Senior Full Stack Developer",
                    Location = "New York, NY",
                    EmploymentType = EmploymentType.FullTime,
                    StartDate = ExperienceStartDate,
                    EndDate = null,
                    IsCurrent = true,
                    Description = "Leading development of enterprise web applications using ASP.NET Core and Angular. Responsible for architecture decisions, code reviews, and mentoring junior developers.",
                    Achievements = "[\"Led development of 3 major projects\", \"Improved system performance by 40%\", \"Mentored 5 junior developers\"]",
                    Technologies = "[\"C#\", \"ASP.NET Core\", \"Angular\", \"SQL Server\", \"Azure\"]",
                    DisplayOrder = 1,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Experience
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    CompanyName = "Digital Innovations LLC",
                    Position = "Full Stack Developer",
                    Location = "San Francisco, CA",
                    EmploymentType = EmploymentType.FullTime,
                    StartDate = Experience2StartDate,
                    EndDate = ExperienceEndDate,
                    IsCurrent = false,
                    Description = "Developed and maintained web applications using modern technologies. Collaborated with cross-functional teams to deliver high-quality software solutions.",
                    Achievements = "[\"Delivered 10+ projects on time\", \"Reduced bug reports by 30%\", \"Implemented CI/CD pipeline\"]",
                    Technologies = "[\"JavaScript\", \"Node.js\", \"React\", \"MongoDB\", \"Docker\"]",
                    DisplayOrder = 2,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                }
            );
        }

        private static void SeedTestimonials(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Testimonial>().HasData(
                new Testimonial
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    ClientName = "John Smith",
                    ClientTitle = "Project Manager",
                    ClientCompany = "ABC Corporation",
                    Content = "Excellent work on our portfolio system. The developer delivered high-quality code and met all our requirements on time.",
                    Rating = 5,
                    IsApproved = true,
                    IsFeatured = true,
                    DisplayOrder = 1,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new Testimonial
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    ClientName = "Sarah Johnson",
                    ClientTitle = "CTO",
                    ClientCompany = "XYZ Tech",
                    Content = "Outstanding technical skills and great communication throughout the project. Highly recommended!",
                    Rating = 5,
                    IsApproved = true,
                    IsFeatured = true,
                    DisplayOrder = 2,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                }
            );
        }

        private static void SeedSocialLinks(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocialLink>().HasData(
                new SocialLink
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Platform = "github",
                    DisplayName = "GitHub",
                    Url = "https://github.com/yourusername",
                    IconClass = "fab fa-github",
                    Color = "#333",
                    IsVisible = true,
                    DisplayOrder = 1,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new SocialLink
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Platform = "linkedin",
                    DisplayName = "LinkedIn",
                    Url = "https://linkedin.com/in/yourusername",
                    IconClass = "fab fa-linkedin",
                    Color = "#0077b5",
                    IsVisible = true,
                    DisplayOrder = 2,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new SocialLink
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Platform = "twitter",
                    DisplayName = "Twitter",
                    Url = "https://twitter.com/yourusername",
                    IconClass = "fab fa-twitter",
                    Color = "#1da1f2",
                    IsVisible = true,
                    DisplayOrder = 3,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                }
            );
        }

        private static void SeedSiteContent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteContent>().HasData(
                new SiteContent
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Section = "hero_title",
                    ContentType = "text",
                    Content = "Full Stack Developer",
                    IsPublished = true,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new SiteContent
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Section = "hero_subtitle",
                    ContentType = "text",
                    Content = "Building modern web applications with passion and precision",
                    IsPublished = true,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new SiteContent
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Section = "about_intro",
                    ContentType = "html",
                    Content = "<p>I'm a passionate full stack developer with expertise in modern web technologies. I love creating efficient, scalable solutions that make a difference.</p>",
                    IsPublished = true,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                }
            );
        }

        private static void SeedEmailTemplates(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailTemplate>().HasData(
                new EmailTemplate
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Name = "contact_response",
                    Subject = "Thank you for your inquiry",
                    HtmlContent = "<h2>Thank you for contacting me!</h2><p>I've received your message and will get back to you soon.</p>",
                    TextContent = "Thank you for contacting me! I've received your message and will get back to you soon.",
                    IsActive = true,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                },
                new EmailTemplate
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Name = "project_inquiry",
                    Subject = "Project Inquiry Response",
                    HtmlContent = "<h2>Thank you for your project inquiry!</h2><p>I'm excited to learn more about your project needs.</p>",
                    TextContent = "Thank you for your project inquiry! I'm excited to learn more about your project needs.",
                    IsActive = true,
                    CreatedAt = SeedCreatedAt,
                    UpdatedAt = SeedUpdatedAt
                }
            );
        }

        private static void SeedSettings(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Settings>().HasData(
                new Settings { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Key = "site_title", Value = "My Portfolio" },
                new Settings { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Key = "site_description", Value = "Full Stack Developer Portfolio" },
                new Settings { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Key = "contact_email", Value = "contact@portfolio.com" },
                new Settings { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Key = "github_url", Value = "https://github.com/yourusername" },
                new Settings { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Key = "linkedin_url", Value = "https://linkedin.com/in/yourusername" }
            );
        }
    }
}