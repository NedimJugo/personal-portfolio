using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Models.Entities;
using PortfolioApi.Services.Interfaces;
using System.Security.Claims;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ContactController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Submit contact message (public endpoint)
        /// </summary>
        /// <param name="dto">Contact message data</param>
        /// <returns>Success response</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<bool>>> SubmitContactMessage([FromBody] ContactMessageDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var contactMessage = _mapper.Map<ContactMessage>(dto);
                await _unitOfWork.Repository<ContactMessage>().AddAsync(contactMessage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Contact message received from {Email}", dto.Email);

                return Ok(new ApiResponse<bool>
                {
                    Data = true,
                    Message = "Thank you for your message! We'll get back to you soon."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting contact message from {Email}", dto.Email);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while sending your message. Please try again later."
                });
            }
        }

        /// <summary>
        /// Get all contact messages (admin only)
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="status">Filter by status</param>
        /// <returns>Paginated contact messages</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResponse<ContactMessageResponseDTO>>> GetContactMessages(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] ContactStatus? status = null)
        {
            try
            {
                pageSize = Math.Min(pageSize, 100);
                page = Math.Max(page, 1);

                var query = _unitOfWork.Repository<ContactMessage>()
                    .FindAsync(cm => status == null || cm.Status == status);

                var allMessages = (await query).OrderByDescending(cm => cm.CreatedAt);
                var totalCount = allMessages.Count();

                var messages = allMessages
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(cm => new ContactMessageResponseDTO
                    {
                        Id = cm.Id,
                        Name = cm.Name,
                        Email = cm.Email,
                        Subject = cm.Subject,
                        Message = cm.Message,
                        Status = cm.Status,
                        CreatedAt = cm.CreatedAt,
                        HandledById = cm.HandledById
                    })
                    .ToList();

                return Ok(new PaginatedResponse<ContactMessageResponseDTO>
                {
                    Data = messages,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact messages");
                return StatusCode(500, new PaginatedResponse<ContactMessageResponseDTO>
                {
                    Data = new List<ContactMessageResponseDTO>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                });
            }
        }

        /// <summary>
        /// Update contact message status (admin only)
        /// </summary>
        /// <param name="id">Message ID</param>
        /// <param name="dto">Status update data</param>
        /// <returns>Success response</returns>
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateMessageStatus(int id, [FromBody] UpdateContactStatusDTO dto)
        {
            try
            {
                var message = await _unitOfWork.Repository<ContactMessage>().GetByIdAsync(id);
                if (message == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Contact message not found"
                    });
                }

                message.Status = dto.Status;
                if (dto.Status == ContactStatus.Closed)
                {
                    message.HandledById = GetCurrentUserId();
                }

                _unitOfWork.Repository<ContactMessage>().Update(message);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponse<bool> { Data = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact message status for ID: {Id}", id);
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
