using Microsoft.Extensions.Logging;
using Portfolio.Services.Database.Entities;
using Portfolio.Services.Database;
using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Portfolio.Models.Responses;

namespace Portfolio.Services.Services
{
    public class EmailSyncService : IEmailSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailSyncService> _logger;

        public EmailSyncService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<EmailSyncService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SyncEmailsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting email sync...");

                var newEmails = await _emailService.FetchNewEmailsAsync(cancellationToken);
                var importedCount = 0;

                foreach (var email in newEmails)
                {
                    // Check if email already exists
                    var exists = await _context.ContactMessages
                        .AnyAsync(cm => cm.Source == $"email:{email.MessageId}", cancellationToken);

                    if (exists)
                    {
                        _logger.LogDebug("Email {MessageId} already imported, skipping", email.MessageId);
                        continue;
                    }

                    // Check if this is a reply to an existing message
                    if (email.IsReply && !string.IsNullOrEmpty(email.InReplyTo))
                    {
                        var handled = await HandleEmailReplyAsync(email, cancellationToken);
                        if (handled)
                        {
                            importedCount++;
                            continue;
                        }
                    }

                    // Create new contact message
                    var contactMessage = new ContactMessage
                    {
                        Id = Guid.NewGuid(),
                        Name = email.FromName,
                        Email = email.From,
                        Subject = email.Subject,
                        Message = email.Body,
                        Source = $"email:{email.MessageId}",
                        Status = "new",
                        Priority = "medium",
                        CreatedAt = email.ReceivedDate,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };

                    _context.ContactMessages.Add(contactMessage);
                    importedCount++;

                    // Mark email as read
                    await _emailService.MarkEmailAsReadAsync(email.MessageId, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Email sync completed. Imported {Count} new messages", importedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email sync");
            }
        }

        public async Task<int> ImportEmailAsContactMessageAsync(string messageId, CancellationToken cancellationToken = default)
        {
            try
            {
                var emails = await _emailService.FetchNewEmailsAsync(cancellationToken);
                var email = emails.FirstOrDefault(e => e.MessageId == messageId);

                if (email == null)
                {
                    _logger.LogWarning("Email with MessageId {MessageId} not found", messageId);
                    return 0;
                }

                var exists = await _context.ContactMessages
                    .AnyAsync(cm => cm.Source == $"email:{email.MessageId}", cancellationToken);

                if (exists)
                {
                    _logger.LogDebug("Email {MessageId} already imported", messageId);
                    return 0;
                }

                var contactMessage = new ContactMessage
                {
                    Id = Guid.NewGuid(),
                    Name = email.FromName,
                    Email = email.From,
                    Subject = email.Subject,
                    Message = email.Body,
                    Source = $"email:{email.MessageId}",
                    Status = "new",
                    Priority = "medium",
                    CreatedAt = email.ReceivedDate,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _context.ContactMessages.Add(contactMessage);
                await _context.SaveChangesAsync(cancellationToken);

                await _emailService.MarkEmailAsReadAsync(email.MessageId, cancellationToken);

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing email {MessageId}", messageId);
                return 0;
            }
        }

        private async Task<bool> HandleEmailReplyAsync(ReceivedEmailResponse email, CancellationToken cancellationToken)
        {
            try
            {
                // Try to find the original message this is replying to
                var originalMessage = await _context.ContactMessages
                    .FirstOrDefaultAsync(cm => cm.Source != null &&
                                              cm.Source.Contains(email.InReplyTo!),
                                        cancellationToken);

                if (originalMessage == null)
                {
                    // Can't find original message, treat as new contact
                    return false;
                }

                // Create a reply record (as if customer replied)
                var reply = new ContactMessageReply
                {
                    Id = Guid.NewGuid(),
                    ContactMessageId = originalMessage.Id,
                    ReplyMessage = email.Body,
                    ReplyToEmail = email.From,
                    Subject = email.Subject,
                    RepliedById = 1, // System user or default admin
                    RepliedAt = email.ReceivedDate,
                    DeliveryStatus = "received",
                    IsInternal = false,
                    CreatedAt = email.ReceivedDate,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _context.Set<ContactMessageReply>().Add(reply);

                // Update original message status
                originalMessage.Status = "replied";
                originalMessage.UpdatedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                await _emailService.MarkEmailAsReadAsync(email.MessageId, cancellationToken);

                _logger.LogInformation("Email reply linked to message {MessageId}", originalMessage.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling email reply");
                return false;
            }
        }
    }
}
