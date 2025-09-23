using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PortfolioApi.Data;
using PortfolioApi.Models.Entities;
using PortfolioApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using PortfolioApi.Helpers;
using PortfolioApi.Services.Implementations;
using PortfolioApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Portfolio API",
        Version = "v1",
        Description = "A comprehensive portfolio management API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var key = Encoding.ASCII.GetBytes(jwtSettings.Get<JwtSettings>()!.SecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Get<JwtSettings>()!.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Get<JwtSettings>()!.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

var corsSettings = builder.Configuration.GetSection("CorsSettings");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database initialization with better error handling and migrations
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting database initialization...");

        // Apply any pending migrations (this will create the database if it doesn't exist)
        await context.Database.MigrateAsync();

        logger.LogInformation("Database migrations applied successfully.");

        // Seed data
        await SeedDataAsync(context, userManager, roleManager, logger);

        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization: {Error}", ex.Message);

        // In development, you might want to see the full error
        if (app.Environment.IsDevelopment())
        {
            logger.LogError("Full exception details: {Exception}", ex.ToString());
        }
    }
}

app.Run();

static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager, ILogger logger)
{
    logger.LogInformation("Starting data seeding...");

    // Create roles
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var result = await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        if (result.Succeeded)
        {
            logger.LogInformation("Admin role created successfully.");
        }
        else
        {
            logger.LogWarning("Failed to create Admin role: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // Create admin user
    var adminEmail = "admin@portfolio.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Portfolio Admin",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Admin user created successfully.");
        }
        else
        {
            logger.LogWarning("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // Seed sample technologies
    if (!context.Techs.Any())
    {
        var techs = new[]
        {
            new Tech { Name = "Angular", Slug = "angular" },
            new Tech { Name = "ASP.NET Core", Slug = "aspnet-core" },
            new Tech { Name = "TypeScript", Slug = "typescript" },
            new Tech { Name = "C#", Slug = "csharp" },
            new Tech { Name = "Entity Framework", Slug = "entity-framework" },
            new Tech { Name = "SQL Server", Slug = "sql-server" },
            new Tech { Name = "Azure", Slug = "azure" },
            new Tech { Name = "Docker", Slug = "docker" }
        };

        context.Techs.AddRange(techs);
        logger.LogInformation("Sample technologies added.");
    }

    // Seed sample tags
    if (!context.Tags.Any())
    {
        var tags = new[]
        {
            new Tag { Name = "Web Development", Slug = "web-development" },
            new Tag { Name = "Full Stack", Slug = "full-stack" },
            new Tag { Name = "API", Slug = "api" },
            new Tag { Name = "SPA", Slug = "spa" },
            new Tag { Name = "Cloud", Slug = "cloud" }
        };

        context.Tags.AddRange(tags);
        logger.LogInformation("Sample tags added.");
    }

    await context.SaveChangesAsync();
    logger.LogInformation("Data seeding completed.");

    // Verification: Check if the admin user has the admin role
    var verifyAdminUser = await userManager.FindByEmailAsync("admin@portfolio.com");
    if (verifyAdminUser != null)
    {
        var hasAdminRole = await userManager.IsInRoleAsync(verifyAdminUser, "Admin");
        var userRoles = await userManager.GetRolesAsync(verifyAdminUser);

        logger.LogInformation("Admin user verification:");
        logger.LogInformation("- User ID: {UserId}", verifyAdminUser.Id);
        logger.LogInformation("- Has Admin Role: {HasAdminRole}", hasAdminRole);
        logger.LogInformation("- All User Roles: {UserRoles}", string.Join(", ", userRoles));
    }
    else
    {
        logger.LogError("Admin user not found during verification!");
    }
}