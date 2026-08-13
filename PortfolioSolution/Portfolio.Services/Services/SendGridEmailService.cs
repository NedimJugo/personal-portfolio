using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Portfolio.Services.Interfaces;
using Portfolio.Services.Database.Entities;
using Portfolio.Models.Responses;

namespace Portfolio.Services.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailService(
            IConfiguration configuration, 
            ILogger<SendGridEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _apiKey = _configuration["SendGrid:ApiKey"] ?? "";
            _fromEmail = _configuration["SendGrid:FromEmail"] ?? "";
            _fromName = _configuration["SendGrid:FromName"] ?? "Portfolio Contact";

            _logger.LogInformation("✨ SendGrid Email Service Initialized - From: {From}", _fromEmail);
        }

        public string GetConfiguredEmail()
        {
            return _fromEmail;
        }

        public async Task<bool> SendEmailAsync(
            string to, 
            string subject, 
            string body, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("❌ SendGrid API key not configured");
                    return false;
                }

                if (string.IsNullOrEmpty(_fromEmail))
                {
                    _logger.LogError("❌ SendGrid FromEmail not configured");
                    return false;
                }

                _logger.LogInformation("📧 Sending email via SendGrid to {To}", to);

                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(_fromEmail, _fromName);
                var toAddress = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, null, body);

                var response = await client.SendEmailAsync(msg, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Email sent successfully via SendGrid to {To}", to);
                    return true;
                }
                else
                {
                    var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("❌ SendGrid error {StatusCode}: {Response}", 
                        response.StatusCode, responseBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending email via SendGrid: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> SendReplyEmailAsync(
            ContactMessageReply reply, 
            CancellationToken cancellationToken = default)
        {
            if (reply.IsInternal || string.IsNullOrEmpty(reply.ReplyToEmail))
            {
                _logger.LogInformation("ℹ️ Skipping email - Internal: {IsInternal}", reply.IsInternal);
                return true;
            }

            var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #4ECDC4;'>Reply to your message</h2>
                    <p><strong>Subject:</strong> {reply.Subject}</p>
                    <hr style='border: 1px solid #ddd; margin: 20px 0;'/>
                    <div style='margin: 20px 0; line-height: 1.6;'>
                        {reply.ReplyMessage.Replace("\n", "<br/>")}
                    </div>
                    <hr style='border: 1px solid #ddd; margin: 20px 0;'/>
                    <p style='color: #666; font-size: 12px;'>
                        This is a reply to your contact form submission.
                    </p>
                </body>
                </html>
            ";

            return await SendEmailAsync(
                reply.ReplyToEmail, 
                reply.Subject ?? "Reply to your message", 
                emailBody, 
                cancellationToken
            );
        }

        public async Task<List<ReceivedEmailResponse>> FetchNewEmailsAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("⚠️ FetchNewEmailsAsync not supported with SendGrid");
            return new List<ReceivedEmailResponse>();
        }

        public async Task<bool> MarkEmailAsReadAsync(
            string messageId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("⚠️ MarkEmailAsReadAsync not supported with SendGrid");
            return await Task.FromResult(false);
        }

        public string AddUnsubscribeLink(string htmlContent, string email, string unsubscribeToken)
        {
            var unsubscribeUrl = $"{_configuration["AppUrl"]}/unsubscribe?token={unsubscribeToken}";
            
            var unsubscribeHtml = $@"
                <hr style='margin: 30px 0; border: 1px solid #ddd;'>
                <div style='text-align: center; padding: 20px; font-family: Arial, sans-serif;'>
                    <p style='font-size: 12px; color: #666; margin: 10px 0;'>
                        <a href='{unsubscribeUrl}' style='color: #4ECDC4; text-decoration: none;'>
                            Unsubscribe from these emails
                        </a>
                    </p>
                </div>
            ";

            return htmlContent + unsubscribeHtml;
        }
    }
}