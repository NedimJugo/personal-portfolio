using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.BaseServices;
using Portfolio.Services.Database.Entities;
using Portfolio.Services.Database;
using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Portfolio.Services.Services
{
    public class ContactMessageReplyService
        : BaseCRUDService<ContactMessageReplyResponse, ContactMessageReplySearchObject, ContactMessageReply,
                          ContactMessageReplyInsertRequest, ContactMessageReplyUpdateRequest, Guid>,
          IContactMessageReplyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        public ContactMessageReplyService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<ContactMessageReplyService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService)
            : base(context, mapper, logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        protected override IQueryable<ContactMessageReply> ApplyIncludes(IQueryable<ContactMessageReply> query)
        {
            return query
                .Include(r => r.ContactMessage)
                .Include(r => r.RepliedBy);
        }

        protected override IQueryable<ContactMessageReply> ApplyFilter(
            IQueryable<ContactMessageReply> query,
            ContactMessageReplySearchObject? search = null)
        {
            if (search == null) return query;

            if (search.ContactMessageId.HasValue)
                query = query.Where(x => x.ContactMessageId == search.ContactMessageId.Value);

            if (search.RepliedById.HasValue)
                query = query.Where(x => x.RepliedById == search.RepliedById.Value);

            if (!string.IsNullOrWhiteSpace(search.DeliveryStatus))
                query = query.Where(x => x.DeliveryStatus == search.DeliveryStatus);

            if (search.IsInternal.HasValue)
                query = query.Where(x => x.IsInternal == search.IsInternal.Value);

            if (search.RepliedFrom.HasValue)
                query = query.Where(x => x.RepliedAt >= search.RepliedFrom.Value);

            if (search.RepliedTo.HasValue)
                query = query.Where(x => x.RepliedAt <= search.RepliedTo.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(
            ContactMessageReply entity,
            ContactMessageReplyInsertRequest request,
            CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.RepliedAt = DateTimeOffset.UtcNow;

            var userIdClaim = _httpContextAccessor.HttpContext?.User
                        .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                entity.RepliedById = userId;
            }
            else
            {
                throw new UnauthorizedAccessException("User must be authenticated to create a reply");
            }

            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(
            ContactMessageReply entity,
            ContactMessageReplyUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task AfterInsertAsync(
    ContactMessageReply entity,
    ContactMessageReplyInsertRequest request,
    CancellationToken cancellationToken = default)
        {
            // Send email if not internal
            if (!entity.IsInternal && !string.IsNullOrEmpty(entity.ReplyToEmail))
            {
                try
                {
                    var emailSent = await _emailService.SendReplyEmailAsync(entity, cancellationToken);

                    entity.DeliveryStatus = emailSent ? "sent" : "failed";
                    entity.DeliveredAt = emailSent ? DateTimeOffset.UtcNow : null;

                    if (!emailSent)
                    {
                        entity.ErrorMessage = "Failed to send email";
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reply email for message {MessageId}", entity.ContactMessageId);
                    entity.DeliveryStatus = "failed";
                    entity.ErrorMessage = ex.Message;
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            await Task.CompletedTask;
        }
    }
}
