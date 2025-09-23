using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Helpers;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Models.Entities;
using PortfolioApi.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Security.Claims;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MediaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MediaController> _logger;
        private readonly IWebHostEnvironment _environment;

        public MediaController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<MediaController> logger,
            IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Get all media files
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Paginated media files</returns>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<MediaDto>>> GetMediaFiles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                pageSize = Math.Min(pageSize, 100);
                page = Math.Max(page, 1);

                var allMedia = (await _unitOfWork.Repository<Media>().GetAllAsync())
                    .OrderByDescending(m => m.UploadedAt);

                var totalCount = allMedia.Count();
                var mediaFiles = allMedia
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var mediaDtos = _mapper.Map<List<MediaDto>>(mediaFiles);

                return Ok(new PaginatedResponse<MediaDto>
                {
                    Data = mediaDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media files");
                return StatusCode(500, new PaginatedResponse<MediaDto>
                {
                    Data = new List<MediaDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                });
            }
        }

        /// <summary>
        /// Upload media file
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <returns>Created media record</returns>
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<MediaDto>>> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<MediaDto>
                    {
                        Success = false,
                        Message = "No file provided"
                    });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".pdf", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new ApiResponse<MediaDto>
                    {
                        Success = false,
                        Message = "File type not allowed"
                    });
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new ApiResponse<MediaDto>
                    {
                        Success = false,
                        Message = "File size too large. Maximum size is 10MB"
                    });
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");

                // Ensure uploads directory exists
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Get image dimensions if it's an image
                int? width = null, height = null;
                if (IsImageFile(fileExtension))
                {
                    try
                    {
                        using (var image = await Image.LoadAsync<Rgba32>(filePath))
                        {
                            width = image.Width;
                            height = image.Height;
                        }
                    }
                    catch
                    {
                        // If we can't read image dimensions, continue without them
                    }
                }

                // Create media record
                var media = new Media
                {
                    FileName = file.FileName,
                    Url = $"/uploads/{fileName}",
                    StorageProvider = "Local",
                    ContentType = file.ContentType,
                    Size = file.Length,
                    Width = width,
                    Height = height,
                    UploadedById = GetCurrentUserId()
                };

                await _unitOfWork.Repository<Media>().AddAsync(media);
                await _unitOfWork.SaveChangesAsync();

                var mediaDto = _mapper.Map<MediaDto>(media);

                return CreatedAtAction(nameof(GetMedia), new { id = media.Id }, new ApiResponse<MediaDto>
                {
                    Data = mediaDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new ApiResponse<MediaDto>
                {
                    Success = false,
                    Message = "An error occurred while uploading the file"
                });
            }
        }

        /// <summary>
        /// Get media file by ID
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <returns>Media details</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<MediaDto>>> GetMedia(int id)
        {
            try
            {
                var media = await _unitOfWork.Repository<Media>().GetByIdAsync(id);
                if (media == null)
                {
                    return NotFound(new ApiResponse<MediaDto>
                    {
                        Success = false,
                        Message = "Media not found"
                    });
                }

                var mediaDto = _mapper.Map<MediaDto>(media);
                return Ok(new ApiResponse<MediaDto> { Data = mediaDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<MediaDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Delete media file
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteMedia(int id)
        {
            try
            {
                var media = await _unitOfWork.Repository<Media>().GetByIdAsync(id);
                if (media == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Media not found"
                    });
                }

                // Check if media is being used
                var isInUse = await _unitOfWork.Repository<Project>()
                    .ExistsAsync(p => p.FeaturedMediaId == id);

                if (isInUse)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete media file that is being used"
                    });
                }

                // Delete physical file
                if (media.StorageProvider == "Local")
                {
                    var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath,
                        media.Url.TrimStart('/'));

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Delete database record
                _unitOfWork.Repository<Media>().Delete(media);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponse<bool> { Data = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media with ID: {Id}", id);
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

        private static bool IsImageFile(string extension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            return imageExtensions.Contains(extension);
        }
    }
}
