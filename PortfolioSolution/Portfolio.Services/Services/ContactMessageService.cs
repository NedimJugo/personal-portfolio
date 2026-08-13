using AutoMapper;
using Microsoft.Extensions.Logging;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.BaseServices;
using Portfolio.Services.Database.Entities;
using Portfolio.Services.Database;
using Portfolio.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Portfolio.Services.Helpers;

namespace Portfolio.Services.Services
{
    public class ContactMessageService
        : BaseCRUDService<ContactMessageResponse, ContactMessageSearchObject, ContactMessage, 
                          ContactMessageInsertRequest, ContactMessageUpdateRequest, Guid>,
          IContactMessageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGeolocationService _geolocationService;
        private readonly IEmailService _emailService;

        public ContactMessageService(
            ApplicationDbContext context, 
            IMapper mapper, 
            ILogger<ContactMessageService> logger, 
            IHttpContextAccessor httpContextAccessor,
            IGeolocationService geolocationService, 
            IEmailService emailService)
            : base(context, mapper, logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _geolocationService = geolocationService;
            _emailService = emailService;
        }

        protected override IQueryable<ContactMessage> ApplyFilter(
            IQueryable<ContactMessage> query, 
            ContactMessageSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(x => x.Name.Contains(search.Name));

            if (!string.IsNullOrWhiteSpace(search.Email))
                query = query.Where(x => x.Email.Contains(search.Email));

            if (!string.IsNullOrWhiteSpace(search.Subject))
                query = query.Where(x => x.Subject.Contains(search.Subject));

            if (!string.IsNullOrWhiteSpace(search.Status))
                query = query.Where(x => x.Status == search.Status);

            if (!string.IsNullOrWhiteSpace(search.Priority))
                query = query.Where(x => x.Priority == search.Priority);

            if (search.HandledById.HasValue)
                query = query.Where(x => x.HandledById == search.HandledById.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(
            ContactMessage entity, 
            ContactMessageInsertRequest request, 
            CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            if (_httpContextAccessor.HttpContext != null)
            {
                entity.IpAddress = IpAddressHelper.GetClientIpAddress(_httpContextAccessor.HttpContext);
            }
            
            await Task.CompletedTask;
        }

        protected override async Task AfterInsertAsync(
            ContactMessage entity, 
            ContactMessageInsertRequest request, 
            CancellationToken cancellationToken = default)
        {
            // Send notification email synchronously with proper error handling
            try
            {
                _logger.LogInformation("Preparing to send notification email for contact message from {Email}", entity.Email);

                var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2 style='color: #4ECDC4;'>New Contact Message Received</h2>
                        <div style='background: #f5f5f5; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>From:</strong> {entity.Name}</p>
                            <p><strong>Email:</strong> <a href='mailto:{entity.Email}'>{entity.Email}</a></p>
                            <p><strong>Subject:</strong> {entity.Subject}</p>
                            <p><strong>Date:</strong> {entity.CreatedAt:yyyy-MM-dd HH:mm:ss}</p>
                            {(!string.IsNullOrEmpty(entity.Phone) ? $"<p><strong>Phone:</strong> {entity.Phone}</p>" : "")}
                            {(!string.IsNullOrEmpty(entity.Company) ? $"<p><strong>Company:</strong> {entity.Company}</p>" : "")}
                            {(!string.IsNullOrEmpty(entity.IpAddress) ? $"<p><strong>IP Address:</strong> {entity.IpAddress}</p>" : "")}
                        </div>
                        <div style='background: white; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                            <h3>Message:</h3>
                            <p style='white-space: pre-wrap;'>{entity.Message}</p>
                        </div>
                        <hr style='margin: 30px 0; border: 1px solid #ddd;'>
                        <p style='color: #666; font-size: 12px;'>
                            This is an automated notification from your portfolio contact form.
                        </p>
                    </body>
                    </html>
                ";

                var notificationEmail = _emailService.GetConfiguredEmail();
                _logger.LogInformation("Sending notification to: {NotificationEmail}", notificationEmail);

                // Use a longer timeout for the notification email
                using var emailCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                var emailSent = await _emailService.SendEmailAsync(
                    notificationEmail,
                    $"New Contact Message: {entity.Subject}",
                    emailBody,
                    emailCts.Token
                );

                if (emailSent)
                {
                    _logger.LogInformation("✅ Contact notification sent successfully for message from {Email}", entity.Email);
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to send contact notification - email service returned false");
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - we don't want email failure to break the contact form
                _logger.LogError(ex, "❌ Error sending contact notification email: {Message}", ex.Message);
            }

            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(
            ContactMessage entity, 
            ContactMessageUpdateRequest request, 
            CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}