using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<ProjectDto>>> GetProjects(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? tag = null,
            [FromQuery] string? tech = null)
        {
            try
            {
                // Limit page size to prevent abuse
                pageSize = Math.Min(pageSize, 50);
                page = Math.Max(page, 1);

                var result = await _projectService.GetProjectsAsync(page, pageSize, search, tag, tech, publishedOnly: true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return StatusCode(500, new PaginatedResponse<ProjectDto>
                {
                    Data = new List<ProjectDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                });
            }
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(string slug)
        {
            try
            {
                var result = await _projectService.GetProjectBySlugAsync(slug, publishedOnly: true);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                // Increment view count asynchronously (fire and forget)
                _ = Task.Run(async () => await _projectService.IncrementViewsAsync(slug));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with slug: {Slug}", slug);
                return StatusCode(500, new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }
    }
}
