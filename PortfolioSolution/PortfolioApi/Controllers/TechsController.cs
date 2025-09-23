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
    public class TechsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TechsController> _logger;

        public TechsController(ApplicationDbContext context, IMapper mapper, ILogger<TechsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all technologies
        /// </summary>
        /// <returns>List of technologies</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TechDto>>>> GetTechs()
        {
            try
            {
                var techs = await _context.Techs
                    .Include(t => t.IconMedia)
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                var techDtos = _mapper.Map<List<TechDto>>(techs);

                return Ok(new ApiResponse<List<TechDto>>
                {
                    Data = techDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving technologies");
                return StatusCode(500, new ApiResponse<List<TechDto>>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Get technology by ID
        /// </summary>
        /// <param name="id">Technology ID</param>
        /// <returns>Technology details</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<TechDto>>> GetTech(int id)
        {
            try
            {
                var tech = await _context.Techs
                    .Include(t => t.IconMedia)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tech == null)
                {
                    return NotFound(new ApiResponse<TechDto>
                    {
                        Success = false,
                        Message = "Technology not found"
                    });
                }

                var techDto = _mapper.Map<TechDto>(tech);

                return Ok(new ApiResponse<TechDto>
                {
                    Data = techDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving technology with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<TechDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Create new technology (admin only)
        /// </summary>
        /// <param name="dto">Technology data</param>
        /// <returns>Created technology</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<TechDto>>> CreateTech([FromBody] CreateTechDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<TechDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var slug = GenerateSlug(dto.Name);
                var existingTech = await _context.Techs.AnyAsync(t => t.Slug == slug);

                if (existingTech)
                {
                    return BadRequest(new ApiResponse<TechDto>
                    {
                        Success = false,
                        Message = "A technology with this name already exists"
                    });
                }

                var tech = new Tech
                {
                    Name = dto.Name,
                    Slug = slug,
                    IconMediaId = dto.IconMediaId
                };

                _context.Techs.Add(tech);
                await _context.SaveChangesAsync();

                // Reload with icon media
                await _context.Entry(tech)
                    .Reference(t => t.IconMedia)
                    .LoadAsync();

                var techDto = _mapper.Map<TechDto>(tech);

                return CreatedAtAction(nameof(GetTech), new { id = tech.Id }, new ApiResponse<TechDto>
                {
                    Data = techDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating technology");
                return StatusCode(500, new ApiResponse<TechDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Update existing technology (admin only)
        /// </summary>
        /// <param name="id">Technology ID</param>
        /// <param name="dto">Technology update data</param>
        /// <returns>Updated technology</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<TechDto>>> UpdateTech(int id, [FromBody] UpdateTechDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<TechDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var tech = await _context.Techs.FindAsync(id);
                if (tech == null)
                {
                    return NotFound(new ApiResponse<TechDto>
                    {
                        Success = false,
                        Message = "Technology not found"
                    });
                }

                var newSlug = GenerateSlug(dto.Name);
                var existingTech = await _context.Techs.AnyAsync(t => t.Slug == newSlug && t.Id != id);

                if (existingTech)
                {
                    return BadRequest(new ApiResponse<TechDto>
                    {
                        Success = false,
                        Message = "A technology with this name already exists"
                    });
                }

                tech.Name = dto.Name;
                tech.Slug = newSlug;
                tech.IconMediaId = dto.IconMediaId;

                _context.Techs.Update(tech);
                await _context.SaveChangesAsync();

                // Reload with icon media
                await _context.Entry(tech)
                    .Reference(t => t.IconMedia)
                    .LoadAsync();

                var techDto = _mapper.Map<TechDto>(tech);

                return Ok(new ApiResponse<TechDto>
                {
                    Data = techDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating technology with ID: {Id}", id);
                return StatusCode(500, new ApiResponse<TechDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Delete technology (admin only)
        /// </summary>
        /// <param name="id">Technology ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTech(int id)
        {
            try
            {
                var tech = await _context.Techs.FindAsync(id);
                if (tech == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Technology not found"
                    });
                }

                // Check if technology is being used by any projects
                var isInUse = await _context.ProjectTechs.AnyAsync(pt => pt.TechId == id);
                if (isInUse)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete technology that is being used by projects"
                    });
                }

                _context.Techs.Remove(tech);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<bool> { Data = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting technology with ID: {Id}", id);
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
                .Replace(".", "")
                .Replace("#", "sharp")
                .Replace("+", "plus")
                .Replace("&", "and")
                .Replace("(", "")
                .Replace(")", "");
        }
    }
}
