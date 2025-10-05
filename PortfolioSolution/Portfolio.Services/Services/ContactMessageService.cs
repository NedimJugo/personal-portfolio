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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Portfolio.Services.Helpers;

namespace Portfolio.Services.Services
{
    public class ContactMessageService
        : BaseCRUDService<ContactMessageResponse, ContactMessageSearchObject, ContactMessage, ContactMessageInsertRequest, ContactMessageUpdateRequest, Guid>,
          IContactMessageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGeolocationService _geolocationService;
        public ContactMessageService(ApplicationDbContext context, IMapper mapper, ILogger<ContactMessageService> logger, IHttpContextAccessor httpContextAccessor,
            IGeolocationService geolocationService)
            : base(context, mapper, logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _geolocationService = geolocationService;
        }


        protected override IQueryable<ContactMessage> ApplyFilter(IQueryable<ContactMessage> query, ContactMessageSearchObject? search = null)
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

        protected override async Task BeforeInsertAsync(ContactMessage entity, ContactMessageInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            if (_httpContextAccessor.HttpContext != null)
            {
                entity.IpAddress = IpAddressHelper.GetClientIpAddress(_httpContextAccessor.HttpContext);
            }
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(ContactMessage entity, ContactMessageUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
