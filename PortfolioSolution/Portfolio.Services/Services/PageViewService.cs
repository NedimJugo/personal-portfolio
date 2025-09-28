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
    public class PageViewService
        : BaseCRUDService<PageViewResponse, PageViewSearchObject, PageView, PageViewInsertRequest, PageViewUpdateRequest, Guid>,
          IPageViewService
    {
        public PageViewService(ApplicationDbContext context, IMapper mapper, ILogger<PageViewService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<PageView> ApplyFilter(IQueryable<PageView> query, PageViewSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Path))
                query = query.Where(x => x.Path.Contains(search.Path));

            if (!string.IsNullOrWhiteSpace(search.IpAddress))
                query = query.Where(x => x.IpAddress == search.IpAddress);

            if (!string.IsNullOrWhiteSpace(search.Country))
                query = query.Where(x => x.Country == search.Country);

            if (search.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == search.ProjectId.Value);

            if (search.BlogPostId.HasValue)
                query = query.Where(x => x.BlogPostId == search.BlogPostId.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(PageView entity, PageViewInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.ViewedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(PageView entity, PageViewUpdateRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}
