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
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Services.Services
{
    public class PageViewService
        : BaseCRUDService<PageViewResponse, PageViewSearchObject, PageView, PageViewInsertRequest, PageViewUpdateRequest, Guid>,
          IPageViewService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGeolocationService _geolocationService;
        public PageViewService(ApplicationDbContext context, IMapper mapper, ILogger<PageViewService> logger, IHttpContextAccessor httpContextAccessor,
            IGeolocationService geolocationService)
            : base(context, mapper, logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _geolocationService = geolocationService;
        }


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

            if (_httpContextAccessor.HttpContext != null)
            {
                entity.IpAddress = IpAddressHelper.GetClientIpAddress(_httpContextAccessor.HttpContext);

                // Get geolocation if we have an IP
                if (!string.IsNullOrEmpty(entity.IpAddress))
                {
                    var (country, city) = await _geolocationService.GetLocationFromIpAsync(entity.IpAddress);
                    entity.Country = country;
                    entity.City = city;
                }
            }

            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(PageView entity, PageViewUpdateRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected override async Task AfterInsertAsync(PageView entity, PageViewInsertRequest request, CancellationToken cancellationToken = default)
        {
            // Increment blog post view count if this is a blog post view
            if (entity.BlogPostId.HasValue)
            {
                var blogPost = await _context.BlogPosts
                    .FirstOrDefaultAsync(bp => bp.Id == entity.BlogPostId.Value, cancellationToken);

                if (blogPost != null)
                {
                    blogPost.ViewCount++;
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Incremented view count for blog post {BlogPostId}. New count: {ViewCount}",
                        blogPost.Id, blogPost.ViewCount);
                }
                else
                {
                    _logger.LogWarning("Blog post {BlogPostId} not found for view count increment", entity.BlogPostId.Value);
                }
            }

            // Similarly for projects if needed
            if (entity.ProjectId.HasValue)
            {
                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == entity.ProjectId.Value, cancellationToken);

                if (project != null)
                {
                    project.ViewCount++;
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Incremented view count for project {ProjectId}. New count: {ViewCount}",
                        project.Id, project.ViewCount);
                }
            }

            await base.AfterInsertAsync(entity, request, cancellationToken);
        }
    }
}
