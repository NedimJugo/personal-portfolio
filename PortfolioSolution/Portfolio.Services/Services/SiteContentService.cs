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
    public class SiteContentService
        : BaseCRUDService<SiteContentResponse, SiteContentSearchObject, SiteContent, SiteContentInsertRequest, SiteContentUpdateRequest, Guid>,
          ISiteContentService
    {
        public SiteContentService(ApplicationDbContext context, IMapper mapper, ILogger<SiteContentService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<SiteContent> ApplyFilter(IQueryable<SiteContent> query, SiteContentSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Section))
                query = query.Where(x => x.Section.Contains(search.Section));

            if (!string.IsNullOrWhiteSpace(search.ContentType))
                query = query.Where(x => x.ContentType == search.ContentType);

            if (search.IsPublished.HasValue)
                query = query.Where(x => x.IsPublished == search.IsPublished.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(SiteContent entity, SiteContentInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(SiteContent entity, SiteContentUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
