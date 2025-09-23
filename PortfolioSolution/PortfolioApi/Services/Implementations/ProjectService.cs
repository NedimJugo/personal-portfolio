using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Models.Entities;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectService> _logger;
        private readonly ApplicationDbContext _context;

        public ProjectService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProjectService> logger,
            ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public async Task<PaginatedResponse<ProjectDto>> GetProjectsAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            string? tag = null,
            string? tech = null,
            bool publishedOnly = true)
        {
            try
            {
                var query = _context.Projects
                    .Include(p => p.FeaturedMedia)
                    .Include(p => p.ProjectTechs)
                        .ThenInclude(pt => pt.Tech)
                            .ThenInclude(t => t.IconMedia)
                    .Include(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
                    .AsQueryable();

                if (publishedOnly)
                {
                    query = query.Where(p => p.IsPublished && p.PublishedAt <= DateTimeOffset.UtcNow);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p => p.Title.Contains(search) ||
                                           p.ShortDescription.Contains(search) ||
                                           p.Content.Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    query = query.Where(p => p.ProjectTags.Any(pt => pt.Tag.Slug == tag));
                }

                if (!string.IsNullOrWhiteSpace(tech))
                {
                    query = query.Where(p => p.ProjectTechs.Any(pt => pt.Tech.Slug == tech));
                }

                var totalCount = await query.CountAsync();

                var projects = await query
                    .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var projectDtos = _mapper.Map<List<ProjectDto>>(projects);

                return new PaginatedResponse<ProjectDto>
                {
                    Data = projectDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return new PaginatedResponse<ProjectDto>
                {
                    Data = new List<ProjectDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
        }

        public async Task<ApiResponse<ProjectDto>> GetProjectBySlugAsync(string slug, bool publishedOnly = true)
        {
            try
            {
                var query = _context.Projects
                    .Include(p => p.FeaturedMedia)
                    .Include(p => p.ProjectTechs)
                        .ThenInclude(pt => pt.Tech)
                            .ThenInclude(t => t.IconMedia)
                    .Include(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.Slug == slug);

                if (publishedOnly)
                {
                    query = query.Where(p => p.IsPublished && p.PublishedAt <= DateTimeOffset.UtcNow);
                }

                var project = await query.FirstOrDefaultAsync();

                if (project == null)
                {
                    return new ApiResponse<ProjectDto>
                    {
                        Success = false,
                        Message = "Project not found"
                    };
                }

                var projectDto = _mapper.Map<ProjectDto>(project);

                return new ApiResponse<ProjectDto>
                {
                    Data = projectDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project by slug {Slug}", slug);
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the project"
                };
            }
        }

        public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(int id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.FeaturedMedia)
                    .Include(p => p.ProjectTechs)
                        .ThenInclude(pt => pt.Tech)
                            .ThenInclude(t => t.IconMedia)
                    .Include(p => p.ProjectTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return new ApiResponse<ProjectDto>
                    {
                        Success = false,
                        Message = "Project not found"
                    };
                }

                var projectDto = _mapper.Map<ProjectDto>(project);

                return new ApiResponse<ProjectDto>
                {
                    Data = projectDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project by ID {Id}", id);
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the project"
                };
            }
        }

        public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, int userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Generate slug
                var slug = GenerateSlug(dto.Title);
                var existingSlug = await _unitOfWork.Repository<Project>()
                    .ExistsAsync(p => p.Slug == slug);

                if (existingSlug)
                {
                    slug = $"{slug}-{DateTime.UtcNow.Ticks}";
                }

                var project = new Project
                {
                    Title = dto.Title,
                    Slug = slug,
                    ShortDescription = dto.ShortDescription,
                    Content = dto.Content,
                    FeaturedMediaId = dto.FeaturedMediaId,
                    RepoUrl = dto.RepoUrl,
                    LiveUrl = dto.LiveUrl,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsPublished = dto.IsPublished,
                    PublishedAt = dto.IsPublished ? DateTimeOffset.UtcNow : null,
                    CreatedById = userId,
                    UpdatedById = userId
                };

                await _unitOfWork.Repository<Project>().AddAsync(project);
                await _unitOfWork.SaveChangesAsync();

                // Add tech relationships
                if (dto.TechIds.Any())
                {
                    var projectTechs = dto.TechIds.Select(techId => new ProjectTech
                    {
                        ProjectId = project.Id,
                        TechId = techId
                    });

                    await _unitOfWork.Repository<ProjectTech>().AddRangeAsync(projectTechs);
                }

                // Add tag relationships
                if (dto.TagIds.Any())
                {
                    var projectTags = dto.TagIds.Select(tagId => new ProjectTag
                    {
                        ProjectId = project.Id,
                        TagId = tagId
                    });

                    await _unitOfWork.Repository<ProjectTag>().AddRangeAsync(projectTags);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Reload project with relationships
                var createdProject = await GetProjectByIdAsync(project.Id);

                return createdProject;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating project");
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the project"
                };
            }
        }

        public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(int id, UpdateProjectDto dto, int userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var project = await _context.Projects
                    .Include(p => p.ProjectTechs)
                    .Include(p => p.ProjectTags)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return new ApiResponse<ProjectDto>
                    {
                        Success = false,
                        Message = "Project not found"
                    };
                }

                // Update basic fields
                project.Title = dto.Title;
                project.ShortDescription = dto.ShortDescription;
                project.Content = dto.Content;
                project.FeaturedMediaId = dto.FeaturedMediaId;
                project.RepoUrl = dto.RepoUrl;
                project.LiveUrl = dto.LiveUrl;
                project.StartDate = dto.StartDate;
                project.EndDate = dto.EndDate;

                // Handle publish status change
                if (dto.IsPublished && !project.IsPublished)
                {
                    project.PublishedAt = DateTimeOffset.UtcNow;
                }
                else if (!dto.IsPublished && project.IsPublished)
                {
                    project.PublishedAt = null;
                }

                project.IsPublished = dto.IsPublished;
                project.UpdatedById = userId;
                project.UpdatedAt = DateTimeOffset.UtcNow;

                // Generate new slug if title changed
                var newSlug = GenerateSlug(dto.Title);
                if (newSlug != project.Slug)
                {
                    var existingSlug = await _unitOfWork.Repository<Project>()
                        .ExistsAsync(p => p.Slug == newSlug && p.Id != id);

                    if (!existingSlug)
                    {
                        project.Slug = newSlug;
                    }
                }

                // Update tech relationships
                _context.ProjectTechs.RemoveRange(project.ProjectTechs);
                if (dto.TechIds.Any())
                {
                    var projectTechs = dto.TechIds.Select(techId => new ProjectTech
                    {
                        ProjectId = project.Id,
                        TechId = techId
                    });

                    await _context.ProjectTechs.AddRangeAsync(projectTechs);
                }

                // Update tag relationships
                _context.ProjectTags.RemoveRange(project.ProjectTags);
                if (dto.TagIds.Any())
                {
                    var projectTags = dto.TagIds.Select(tagId => new ProjectTag
                    {
                        ProjectId = project.Id,
                        TagId = tagId
                    });

                    await _context.ProjectTags.AddRangeAsync(projectTags);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Reload project with relationships
                var updatedProject = await GetProjectByIdAsync(project.Id);

                return updatedProject;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating project with ID {Id}", id);
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "An error occurred while updating the project"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectAsync(int id)
        {
            try
            {
                var project = await _unitOfWork.Repository<Project>().GetByIdAsync(id);
                if (project == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Project not found"
                    };
                }

                _unitOfWork.Repository<Project>().Delete(project);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project with ID {Id}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the project"
                };
            }
        }

        public async Task<ApiResponse<bool>> IncrementViewsAsync(string slug)
        {
            try
            {
                var project = await _unitOfWork.Repository<Project>()
                    .GetFirstAsync(p => p.Slug == slug);

                if (project != null)
                {
                    project.Views++;
                    _unitOfWork.Repository<Project>().Update(project);
                    await _unitOfWork.SaveChangesAsync();
                }

                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing views for project {Slug}", slug);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while updating project views"
                };
            }
        }

        private static string GenerateSlug(string title)
        {
            return title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("?", "")
                .Replace("!", "")
                .Replace("@", "")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("*", "")
                .Replace("+", "")
                .Replace("=", "")
                .Replace("|", "")
                .Replace("`", "")
                .Replace("~", "")
                .Replace("<", "")
                .Replace(">", "");
        }
    }
}
