using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Models.Entities;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ApplicationDbContext context, IMapper mapper, ILogger<TagsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all tags
        /// </summary>
        /// <returns>List of tags</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TagDto>>>> GetTags()
        {
            try
            {
                var tags = await _context.Tags
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                var tagDtos = _mapper.Map<List<TagDto>>(tags);

                return Ok(new ApiResponse<List<TagDto>>
                {
                    Data = tagDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tags");
                return StatusCode(500, new ApiResponse<List<TagDto>>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Get tag by ID
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <returns>Tag details</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<TagDto>>> GetTag(int id)
        {
            try
            {
                var tag = await _context.Tags.FindAsync(id);

                if (tag == null)
                {
                    return NotFound(new ApiResponse<TagDto>
                    {
                        Success = false,
                        Message = "Tag not found"
                    });
                }

                var tagDto = _mapper.Map<TagDto>(tag);

                return Ok(new ApiResponse<TagDto>
                {
                    Data = tagDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<TagDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Create new tag (admin only)
        /// </summary>
        /// <param name="dto">Tag data</param>
        /// <returns>Created tag</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<TagDto>>> CreateTag([FromBody] CreateTagDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<TagDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var slug = GenerateSlug(dto.Name);
                var existingTag = await _context.Tags.AnyAsync(t => t.Slug == slug);

                if (existingTag)
                {
                    return BadRequest(new ApiResponse<TagDto>
                    {
                        Success = false,
                        Message = "A tag with this name already exists"
                    });
                }

                var tag = new Tag
                {
                    Name = dto.Name,
                    Slug = slug
                };

                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

                var tagDto = _mapper.Map<TagDto>(tag);

                return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, new ApiResponse<TagDto>
                {
                    Data = tagDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return StatusCode(500, new ApiResponse<TagDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Update existing tag (admin only)
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <param name="dto">Tag update data</param>
        /// <returns>Updated tag</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<TagDto>>> UpdateTag(int id, [FromBody] UpdateTagDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<TagDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var tag = await _context.Tags.FindAsync(id);
                if (tag == null)
                {
                    return NotFound(new ApiResponse<TagDto>
                    {
                        Success = false,
                        Message = "Tag not found"
                    });
                }

                var newSlug = GenerateSlug(dto.Name);
                var existingTag = await _context.Tags.AnyAsync(t => t.Slug == newSlug && t.Id != id);

                if (existingTag)
                {
                    return BadRequest(new ApiResponse<TagDto>
                    {
                        Success = false,
                        Message = "A tag with this name already exists"
                    });
                }

                tag.Name = dto.Name;
                tag.Slug = newSlug;

                _context.Tags.Update(tag);
                await _context.SaveChangesAsync();

                var tagDto = _mapper.Map<TagDto>(tag);

                return Ok(new ApiResponse<TagDto>
                {
                    Data = tagDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<TagDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Delete tag (admin only)
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTag(int id)
        {
            try
            {
                var tag = await _context.Tags.FindAsync(id);
                if (tag == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Tag not found"
                    });
                }

                // Check if tag is being used by any projects
                var isInUse = await _context.ProjectTags.AnyAsync(pt => pt.TagId == id);
                if (isInUse)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete tag that is being used by projects"
                    });
                }

                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<bool> { Data = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        private static string GenerateSlug(string name)
        {
            return name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("(", "")
                .Replace(")", "");
        }
    }
}
