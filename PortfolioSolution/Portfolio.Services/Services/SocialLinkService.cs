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
    public class SocialLinkService
        : BaseCRUDService<SocialLinkResponse, SocialLinkSearchObject, SocialLink, SocialLinkInsertRequest, SocialLinkUpdateRequest, Guid>,
          ISocialLinkService
    {
        public SocialLinkService(ApplicationDbContext context, IMapper mapper, ILogger<SocialLinkService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<SocialLink> ApplyFilter(IQueryable<SocialLink> query, SocialLinkSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Platform))
                query = query.Where(x => x.Platform.Contains(search.Platform));

            if (search.IsVisible.HasValue)
                query = query.Where(x => x.IsVisible == search.IsVisible.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(SocialLink entity, SocialLinkInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(SocialLink entity, SocialLinkUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
