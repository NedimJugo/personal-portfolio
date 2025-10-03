using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.Interfaces;
using Portfolio.WebAPI.BaseContoller;

namespace Portfolio.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostLikesController
        : BaseCRUDController<BlogPostLikeResponse, BlogPostLikeSearchObject, BlogPostLikeInsertRequest, object, Guid>
    {
        private new readonly IBlogPostLikeService _service;

        public BlogPostLikesController(IBlogPostLikeService service, ILogger<BlogPostLikesController> logger)
            : base(service, logger)
        {
            _service = service;
        }

        [HttpPost("toggle")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [AllowAnonymous]
        public async Task<ActionResult<BlogPostLikeStatusResponse>> ToggleLike(
            [FromQuery] Guid blogPostId,
            [FromQuery] string visitorKey,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (blogPostId == Guid.Empty || string.IsNullOrWhiteSpace(visitorKey))
                {
                    return BadRequest("BlogPostId and VisitorKey are required");
                }

                var result = await _service.ToggleLikeAsync(blogPostId, visitorKey, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for blog post {BlogPostId}", blogPostId);
                return BadRequest("An error occurred while processing your request.");
            }
        }

        [HttpGet("status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [AllowAnonymous]
        public async Task<ActionResult<BlogPostLikeStatusResponse>> GetLikeStatus(
            [FromQuery] Guid blogPostId,
            [FromQuery] string visitorKey,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (blogPostId == Guid.Empty || string.IsNullOrWhiteSpace(visitorKey))
                {
                    return BadRequest("BlogPostId and VisitorKey are required");
                }

                var result = await _service.GetLikeStatusAsync(blogPostId, visitorKey, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like status for blog post {BlogPostId}", blogPostId);
                return BadRequest("An error occurred while processing your request.");
            }
        }
    }
}
