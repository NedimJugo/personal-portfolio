using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Services.Interfaces;

namespace Portfolio.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class UnsubscribeController : ControllerBase
    {
        private readonly ISubscriberService _subscriberService;
        private readonly IUnsubscribeTokenService _tokenService;
        private readonly ILogger<UnsubscribeController> _logger;

        public UnsubscribeController(
            ISubscriberService subscriberService,
            IUnsubscribeTokenService tokenService,
            ILogger<UnsubscribeController> logger)
        {
            _subscriberService = subscriberService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Unsubscribe(
            [FromQuery] string token,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var email = _tokenService.DecryptEmail(token);

                var subscribers = await _subscriberService.GetAsync(
                    new Models.SearchObjects.SubscriberSearchObject
                    {
                        Email = email,
                        PageSize = 1
                    },
                    cancellationToken);

                var subscriber = subscribers.Items.FirstOrDefault();
                if (subscriber == null)
                    return NotFound("Subscriber not found");

                // subscriber.Id is already a Guid type in SubscriberResponse
                await _subscriberService.UpdateAsync(
                    subscriber.Id,  // Use it directly
                    new Models.Requests.UpdateRequests.SubscriberUpdateRequest
                    {
                        Email = subscriber.Email,
                        Name = subscriber.Name,
                        IsActive = false,
                        Source = subscriber.Source,
                        UnsubscribedAt = DateTimeOffset.UtcNow
                    },
                    cancellationToken);

                return Ok(new { message = "Successfully unsubscribed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing unsubscribe request");
                return BadRequest("Invalid unsubscribe token");
            }
        }
    }
}