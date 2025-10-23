using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Portfolio.Services.Database.Entities;
using MailKit.Search;
using MailKit;
using MimeKit;
using MailKit.Net.Imap;
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

                // Fetch unread emails from the last 30 days
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
                // Strip HTML tags for storage (or keep them if you want)
                return System.Text.RegularExpressions.Regex.Replace(message.HtmlBody, "<.*?>", string.Empty);
            }

            return message.TextBody ?? "";
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage, cancellationToken);
                _logger.LogInformation("Email sent successfully to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                return false;
            }
        }

        public async Task<bool> SendReplyEmailAsync(ContactMessageReply reply, CancellationToken cancellationToken = default)
        {
            if (reply.IsInternal || string.IsNullOrEmpty(reply.ReplyToEmail))
                return true; // Don't send internal notes or if no email

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
    }
}
