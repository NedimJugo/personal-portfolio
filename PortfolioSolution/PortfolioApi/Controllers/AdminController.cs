using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Services.Interfaces;
using System.Security.Claims;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IProjectService projectService, ILogger<AdminController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <returns>Dashboard stats</returns>
        [HttpGet("dashboard/stats")]
        public async Task<ActionResult<ApiResponse<DashboardStatsDTO>>> GetDashboardStats()
        {
            try
            {
                // Get all projects (including unpublished)
                var allProjects = await _projectService.GetProjectsAsync(1, 1000, publishedOnly: false);

                var stats = new DashboardStatsDTO
                {
                    TotalProjects = allProjects.TotalCount,
                    PublishedProjects = allProjects.Data.Count(p => p.IsPublished),
                    DraftProjects = allProjects.Data.Count(p => !p.IsPublished),
                    TotalViews = allProjects.Data.Sum(p => p.Views)
                };

                return Ok(new ApiResponse<DashboardStatsDTO> { Data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return StatusCode(500, new ApiResponse<DashboardStatsDTO>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Get all projects for admin (including unpublished)
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="search">Search term</param>
        /// <returns>Paginated list of all projects</returns>
        [HttpGet("projects")]
        public async Task<ActionResult<PaginatedResponse<ProjectDto>>> GetAllProjects(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                pageSize = Math.Min(pageSize, 100);
                page = Math.Max(page, 1);

                var result = await _projectService.GetProjectsAsync(page, pageSize, search, publishedOnly: false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin projects");
                return StatusCode(500, new PaginatedResponse<ProjectDto>
                {
                    Data = new List<ProjectDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                });
            }
        }

        /// <summary>
        /// Get project by ID for admin
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project details</returns>
        [HttpGet("projects/{id:int}")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(int id)
        {
            try
            {
                var result = await _projectService.GetProjectByIdAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Create new project
        /// </summary>
        /// <param name="dto">Project creation data</param>
        /// <returns>Created project</returns>
        [HttpPost("projects")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ProjectDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var userId = GetCurrentUserId();
                var result = await _projectService.CreateProjectAsync(dto, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetProject), new { id = result.Data.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Update existing project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="dto">Project update data</param>
        /// <returns>Updated project</returns>
        [HttpPut("projects/{id:int}")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ProjectDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var userId = GetCurrentUserId();
                var result = await _projectService.UpdateProjectAsync(id, dto, userId);

                if (!result.Success)
                {
                    if (result.Message == "Project not found")
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Delete project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("projects/{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(int id)
        {
            try
            {
                var result = await _projectService.DeleteProjectAsync(id);

                if (!result.Success)
                {
                    if (result.Message == "Project not found")
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }

}
