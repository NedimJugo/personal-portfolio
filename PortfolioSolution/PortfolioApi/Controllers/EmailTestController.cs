using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Services.Interfaces;

namespace Portfolio.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;

        public EmailTestController(
            IEmailService emailService,
            ILogger<EmailTestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("test")]
        [AllowAnonymous] // For testing only - remove in production!
        public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                _logger.LogInformation("Testing email send to {Email}", request.ToEmail);

                var testBody = @"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Email Test</h2>
                        <p>This is a test email from your Portfolio API.</p>
                        <p><strong>Time:</strong> " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + @"</p>
                        <p>If you received this email, your SMTP configuration is working correctly! ✅</p>
                    </body>
                    </html>
                ";

                var result = await _emailService.SendEmailAsync(
                    request.ToEmail,
                    "Portfolio Email Test",
                    testBody
                );

                if (result)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Test email sent successfully",
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Failed to send test email - check server logs for details",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test email endpoint");
                return StatusCode(500, new { 
                    success = false, 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("config")]
        [AllowAnonymous] // For testing only - remove in production!
        public IActionResult GetEmailConfig()
        {
            return Ok(new
            {
                fromEmail = _emailService.GetConfiguredEmail(),
                message = "This is the configured sender email address"
            });
        }

        [HttpGet("diagnose")]
        [AllowAnonymous] // For testing only - remove in production!
        public async Task<IActionResult> DiagnoseSmtpConnection()
        {
            var diagnostics = new List<string>();
            
            try
            {
                diagnostics.Add("🔍 Starting SMTP diagnostics...");
                
                // Test TCP connection to port 587
                try
                {
                    using var client = new System.Net.Sockets.TcpClient();
                    await client.ConnectAsync("smtp.gmail.com", 587);
                    diagnostics.Add("✅ TCP connection to smtp.gmail.com:587 successful");
                    client.Close();
                }
                catch (Exception ex)
                {
                    diagnostics.Add($"❌ TCP connection to port 587 failed: {ex.Message}");
                }
                
                // Test TCP connection to port 465
                try
                {
                    using var client = new System.Net.Sockets.TcpClient();
                    await client.ConnectAsync("smtp.gmail.com", 465);
                    diagnostics.Add("✅ TCP connection to smtp.gmail.com:465 successful");
                    client.Close();
                }
                catch (Exception ex)
                {
                    diagnostics.Add($"❌ TCP connection to port 465 failed: {ex.Message}");
                }
                
                // Test DNS resolution
                try
                {
                    var addresses = await System.Net.Dns.GetHostAddressesAsync("smtp.gmail.com");
                    diagnostics.Add($"✅ DNS resolution successful: {string.Join(", ", addresses.Select(a => a.ToString()))}");
                }
                catch (Exception ex)
                {
                    diagnostics.Add($"❌ DNS resolution failed: {ex.Message}");
                }
                
                return Ok(new
                {
                    success = true,
                    diagnostics = diagnostics,
                    recommendation = "If port 587 is blocked, Railway may be blocking SMTP. Consider using SendGrid or another email service."
                });
            }
            catch (Exception ex)
            {
                diagnostics.Add($"❌ Diagnostic error: {ex.Message}");
                return StatusCode(500, new { success = false, diagnostics = diagnostics });
            }
        }
    }

    public class TestEmailRequest
    {
        public string ToEmail { get; set; } = "";
    }
}