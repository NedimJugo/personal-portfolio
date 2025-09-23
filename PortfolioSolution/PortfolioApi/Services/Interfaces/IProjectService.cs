using PortfolioApi.Models.DTOs;

namespace PortfolioApi.Services.Interfaces
{
    public interface IProjectService
    {
        Task<PaginatedResponse<ProjectDto>> GetProjectsAsync(int page = 1, int pageSize = 10,
            string? search = null, string? tag = null, string? tech = null, bool publishedOnly = true);
        Task<ApiResponse<ProjectDto>> GetProjectBySlugAsync(string slug, bool publishedOnly = true);
        Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(int id);
        Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, int userId);
        Task<ApiResponse<ProjectDto>> UpdateProjectAsync(int id, UpdateProjectDto dto, int userId);
        Task<ApiResponse<bool>> DeleteProjectAsync(int id);
        Task<ApiResponse<bool>> IncrementViewsAsync(string slug);
    }
}
