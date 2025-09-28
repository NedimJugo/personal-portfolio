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

namespace Portfolio.Services.Services
{
    public class SubscriberService
        : BaseCRUDService<SubscriberResponse, SubscriberSearchObject, Subscriber, SubscriberInsertRequest, SubscriberUpdateRequest, Guid>,
          ISubscriberService
    {
        public SubscriberService(ApplicationDbContext context, IMapper mapper, ILogger<SubscriberService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Subscriber> ApplyFilter(IQueryable<Subscriber> query, SubscriberSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Email))
                query = query.Where(x => x.Email.Contains(search.Email));

            if (search.IsActive.HasValue)
                query = query.Where(x => x.IsActive == search.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(search.Source))
                query = query.Where(x => x.Source == search.Source);

            return query;
        }

        protected override async Task BeforeInsertAsync(Subscriber entity, SubscriberInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.SubscribedAt = DateTimeOffset.UtcNow;
            entity.UnsubscribedAt = null;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Subscriber entity, SubscriberUpdateRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.IsActive && entity.UnsubscribedAt == null)
                entity.UnsubscribedAt = DateTimeOffset.UtcNow;

            await Task.CompletedTask;
        }
    }
}
