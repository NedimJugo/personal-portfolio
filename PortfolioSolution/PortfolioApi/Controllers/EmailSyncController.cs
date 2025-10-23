using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Services.Interfaces;

namespace Portfolio.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailSyncController : ControllerBase
    {
        private readonly IEmailSyncService _emailSyncService;
        private readonly ILogger<EmailSyncController> _logger;

        public EmailSyncController(
            IEmailSyncService emailSyncService,
            ILogger<EmailSyncController> logger)
        {
            _emailSyncService = emailSyncService;
            _logger = logger;
        }
        [HttpPost("sync")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> SyncEmails(CancellationToken cancellationToken = default)
        {
            try
            {
                await _emailSyncService.SyncEmailsAsync(cancellationToken);
                return Ok(new { message = "Email sync completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual email sync");
                return StatusCode(500, new { message = "Email sync failed" });
            }
        }

        [HttpPost("import/{messageId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ImportEmail(string messageId, CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _emailSyncService.ImportEmailAsContactMessageAsync(messageId, cancellationToken);

                if (count == 0)
                    return NotFound(new { message = "Email not found or already imported" });

                return Ok(new { message = "Email imported successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing email {MessageId}", messageId);
                return StatusCode(500, new { message = "Email import failed" });
            }
        }
    }
}
