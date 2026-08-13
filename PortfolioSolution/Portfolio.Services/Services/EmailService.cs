using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Portfolio.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using Portfolio.Services.Database.Entities;
using Portfolio.Models.Responses;

namespace Portfolio.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _imapHost;
        private readonly int _imapPort;
        private readonly bool _imapUseSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:Username"] ?? "";
            _smtpPassword = _configuration["Email:Password"] ?? "";
            _fromEmail = _configuration["Email:FromEmail"] ?? "";
            _fromName = _configuration["Email:FromName"] ?? "Portfolio Contact";

            _imapHost = _configuration["Email:ImapHost"] ?? "imap.gmail.com";
            _imapPort = int.Parse(_configuration["Email:ImapPort"] ?? "993");
            _imapUseSsl = bool.Parse(_configuration["Email:ImapUseSsl"] ?? "true");

            _logger.LogInformation("Email Service Initialized - SMTP: {Host}:{Port}, From: {From}", 
                _smtpHost, _smtpPort, _fromEmail);
        }

        public string GetConfiguredEmail()
        {
            return _fromEmail;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("🔄 Attempting to send email to {To} with subject: {Subject}", to, subject);

                // Validate configuration
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogError("❌ SMTP credentials not configured");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Set timeouts
                client.Timeout = 60000; // 60 seconds
                
                _logger.LogDebug("🔌 Connecting to SMTP server {Host}:{Port}", _smtpHost, _smtpPort);

                try
                {
                    // Try with STARTTLS (port 587)
                    await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⚠️ StartTLS connection failed, trying SSL/TLS on port 465: {Message}", ex.Message);
                    
                    // Fallback to SSL/TLS (port 465)
                    await client.ConnectAsync(_smtpHost, 465, MailKit.Security.SecureSocketOptions.SslOnConnect, cancellationToken);
                }

                _logger.LogDebug("🔐 Authenticating with SMTP server");
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword, cancellationToken);

                _logger.LogDebug("📤 Sending email message");
                await client.SendAsync(message, cancellationToken);

                _logger.LogDebug("📡 Disconnecting from SMTP server");
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("✅ Email sent successfully to {To}", to);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⏱️ Email sending timed out for {To}", to);
                return false;
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                _logger.LogError(ex, "🔒 Authentication failed - check username/password: {Message}", ex.Message);
                return false;
            }
            catch (MailKit.Net.Smtp.SmtpCommandException ex)
            {
                _logger.LogError(ex, "📧 SMTP command error: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
                return false;
            }
            catch (MailKit.Net.Smtp.SmtpProtocolException ex)
            {
                _logger.LogError(ex, "🔌 SMTP protocol error: {Message}", ex.Message);
                return false;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "🌐 Network error - SMTP port may be blocked: {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error sending email to {To}: {Message}", to, ex.Message);
                return false;
            }
        }

        public async Task<bool> SendReplyEmailAsync(ContactMessageReply reply, CancellationToken cancellationToken = default)
        {
            if (reply.IsInternal || string.IsNullOrEmpty(reply.ReplyToEmail))
            {
                _logger.LogInformation("ℹ️ Skipping email send - Internal: {IsInternal}, Email: {Email}", 
                    reply.IsInternal, reply.ReplyToEmail ?? "null");
                return true;
            }

            var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Reply to your message</h2>
                    <p><strong>Subject:</strong> {reply.Subject}</p>
                    <hr />
                    <div style='margin: 20px 0;'>
                        {reply.ReplyMessage.Replace("\n", "<br/>")}
                    </div>
                    <hr />
                    <p style='color: #666; font-size: 12px;'>
                        This is a reply to your contact form submission.
                    </p>
                </body>
                </html>
            ";

            return await SendEmailAsync(reply.ReplyToEmail, reply.Subject ?? "Reply to your message", emailBody, cancellationToken);
        }

        public async Task<List<ReceivedEmailResponse>> FetchNewEmailsAsync(CancellationToken cancellationToken = default)
        {
            var emails = new List<ReceivedEmailResponse>();

            try
            {
                using var client = new ImapClient();

                await client.ConnectAsync(_imapHost, _imapPort, _imapUseSsl, cancellationToken);
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword, cancellationToken);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

                var query = SearchQuery.NotSeen
                    .And(SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-30)));

                var uids = await inbox.SearchAsync(query, cancellationToken);

                foreach (var uid in uids)
                {
                    var message = await inbox.GetMessageAsync(uid, cancellationToken);

                    var email = new ReceivedEmailResponse
                    {
                        MessageId = message.MessageId ?? uid.ToString(),
                        From = message.From.Mailboxes.FirstOrDefault()?.Address ?? "",
                        FromName = message.From.Mailboxes.FirstOrDefault()?.Name ?? "",
                        Subject = message.Subject ?? "(No Subject)",
                        Body = GetEmailBody(message),
                        ReplyTo = message.ReplyTo.Mailboxes.FirstOrDefault()?.Address,
                        ReceivedDate = message.Date,
                        IsReply = message.InReplyTo != null,
                        InReplyTo = message.InReplyTo
                    };

                    emails.Add(email);
                }

                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Fetched {Count} new emails", emails.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch emails");
            }

            return emails;
        }

        public async Task<bool> MarkEmailAsReadAsync(string messageId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = new ImapClient();

                await client.ConnectAsync(_imapHost, _imapPort, _imapUseSsl, cancellationToken);
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword, cancellationToken);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

                var query = SearchQuery.HeaderContains("Message-Id", messageId);
                var uids = await inbox.SearchAsync(query, cancellationToken);

                if (uids.Count > 0)
                {
                    await inbox.AddFlagsAsync(uids, MessageFlags.Seen, true, cancellationToken);
                    await client.DisconnectAsync(true, cancellationToken);
                    return true;
                }

                await client.DisconnectAsync(true, cancellationToken);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark email as read: {MessageId}", messageId);
                return false;
            }
        }

        private string GetEmailBody(MimeMessage message)
        {
            if (message.HtmlBody != null)
            {
                return System.Text.RegularExpressions.Regex.Replace(message.HtmlBody, "<.*?>", string.Empty);
            }

            return message.TextBody ?? "";
        }

        public string AddUnsubscribeLink(string htmlContent, string email, string unsubscribeToken)
        {
            var unsubscribeUrl = $"{_configuration["AppUrl"]}/unsubscribe?token={unsubscribeToken}";

            var unsubscribeHtml = $@"
                <hr style='margin: 30px 0; border: 1px solid #ddd;'>
                <div style='text-align: center; padding: 20px; font-family: Arial, sans-serif;'>
                    <p style='font-size: 12px; color: #666; margin: 10px 0;'>
                        You're receiving this email because you subscribed to our newsletter.
                    </p>
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