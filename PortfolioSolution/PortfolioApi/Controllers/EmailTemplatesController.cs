using Microsoft.AspNetCore.Mvc;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.Interfaces;
using Portfolio.WebAPI.BaseContoller;

namespace Portfolio.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplatesController
        : BaseCRUDController<EmailTemplateResponse, EmailTemplateSearchObject, EmailTemplateInsertRequest, EmailTemplateUpdateRequest, Guid>
    {
        private readonly IEmailService _emailService;
        public EmailTemplatesController(IEmailTemplateService service, IEmailService emailService, ILogger<EmailTemplatesController> logger)
            : base(service, logger)
        {
            _emailService = emailService;
        }

        [HttpPost("{id}/send")]
        public async Task<IActionResult> SendEmailToSubscribers(
    Guid id,
    [FromBody] SendEmailRequest request,
    CancellationToken cancellationToken = default)
        {
            try
            {
                var template = await (_service as IEmailTemplateService).GetByIdAsync(id, cancellationToken);
                if (template == null)
                    return NotFound("Template not found");

                if (!template.IsActive)
                    return BadRequest("Cannot send inactive template");

                var tokenService = HttpContext.RequestServices.GetRequiredService<IUnsubscribeTokenService>();
                var results = new List<EmailSendResult>();

                foreach (var email in request.Emails)
                {
                    // Generate unsubscribe token
                    var token = tokenService.GenerateToken(email);

                    // Add unsubscribe link to HTML content
                    var htmlWithUnsubscribe = _emailService.AddUnsubscribeLink(
                        template.HtmlContent,
                        email,
                        token
                    );

                    var success = await _emailService.SendEmailAsync(
                        email,
                        template.Subject,
                        htmlWithUnsubscribe,
                        cancellationToken);

                    results.Add(new EmailSendResult { Email = email, Success = success });
                }

                return Ok(new
                {
                    TotalSent = results.Count(r => r.Success),
                    TotalFailed = results.Count(r => !r.Success),
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send emails for template {TemplateId}", id);
                return StatusCode(500, "Failed to send emails");
            }
        }
    }
}
