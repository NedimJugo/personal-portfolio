namespace PortfolioApi.Models.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? FeaturedMediaUrl { get; set; }
        public string? RepoUrl { get; set; }
        public string? LiveUrl { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsPublished { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public int Views { get; set; }
        public List<TechDto> Techs { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class CreateProjectDto
    {
        public string Title { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? FeaturedMediaId { get; set; }
        public string? RepoUrl { get; set; }
        public string? LiveUrl { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsPublished { get; set; }
        public List<int> TechIds { get; set; } = new();
        public List<int> TagIds { get; set; } = new();
    }

    public class UpdateProjectDto
    {
        public string Title { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? FeaturedMediaId { get; set; }
        public string? RepoUrl { get; set; }
        public string? LiveUrl { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsPublished { get; set; }
        public List<int> TechIds { get; set; } = new();
        public List<int> TagIds { get; set; } = new();
    }

    public class TechDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }

    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}
