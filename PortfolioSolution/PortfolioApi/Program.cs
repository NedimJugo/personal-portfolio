using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Portfolio.Services.Database;
using Portfolio.Services.Database.Entities;
using Portfolio.Services.Interfaces;
using Portfolio.Services.Services;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Portfolio.Models.Configuration;
using Portfolio.Services.Mapping; // Add this using statement

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationUserProfile).Assembly, typeof(BlogPostProfile).Assembly,
    typeof(ContactMessageProfile).Assembly, typeof(EmailTemplateProfile).Assembly, typeof(ExperienceProfile).Assembly, typeof(MediaProfile).Assembly,
    typeof(PageViewProfile).Assembly, typeof(ProjectImageProfile).Assembly, typeof(ProjectProfile).Assembly, typeof(ProjectTagProfile).Assembly,
    typeof(ProjectTechProfile).Assembly, typeof(SettingsProfile).Assembly, typeof(SiteContentProfile).Assembly, typeof(SkillProfile).Assembly,
    typeof(SocialLinkProfile).Assembly, typeof(SubscriberProfile).Assembly, typeof(TagProfile).Assembly, typeof(TechProfile).Assembly,
    typeof(TestimonialProfile).Assembly, typeof(BlogPostLikeProfile).Assembly);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IGeolocationService, GeolocationService>();


builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IBlogPostService, BlogPostService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IExperienceService, ExperienceService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IPageViewService, PageViewService>();
builder.Services.AddScoped<IProjectImageService, ProjectImageService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectTagService, ProjectTagService>();
builder.Services.AddScoped<IProjectTechService, ProjectTechService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<ISiteContentService, SiteContentService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<ISocialLinkService, SocialLinkService>();
builder.Services.AddScoped<ISubscriberService, SubscriberService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ITechService, TechService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IBlogPostLikeService, BlogPostLikeService>();
builder.Services.AddScoped<IGeolocationService, GeolocationService>();
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

// Update this DbContext configuration to suppress the warning
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
});

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



var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings?.SecretKey ?? throw new InvalidOperationException("JWT SecretKey not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, // Remove delay of token when expire
        RequireExpirationTime = true
    };

    // Handle JWT authentication events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Authentication failed: {Exception}", context.Exception);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("JWT Token validated for user: {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Challenge triggered: {Error}", context.Error);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Add custom authorization policies here if needed
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireUserRole", policy =>
        policy.RequireRole("User", "Admin"));
});

// CORS
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
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database initialization - now simplified since seeding is in ModelBuilderExtensions
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting database initialization...");

        // Apply any pending migrations (this will create the database if it doesn't exist)
        // Seeding will happen automatically through ModelBuilder configuration
        await context.Database.MigrateAsync();

        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization: {Error}", ex.Message);

        if (app.Environment.IsDevelopment())
        {
            logger.LogError("Full exception details: {Exception}", ex.ToString());
        }
    }
}

var serviceProvider = app.Services;
_ = Task.Run(async () =>
{
    while (true)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
            await jwtService.CleanupExpiredRefreshTokensAsync();
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error cleaning up expired refresh tokens");
        }

        // Run cleanup every 24 hours
        await Task.Delay(TimeSpan.FromHours(24));
    }
});

app.Run();