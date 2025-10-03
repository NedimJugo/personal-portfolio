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
    public class BlogPostService
        : BaseCRUDService<BlogPostResponse, BlogPostSearchObject, BlogPost, BlogPostInsertRequest, BlogPostUpdateRequest, Guid>,
          IBlogPostService
    {
        public BlogPostService(ApplicationDbContext context, IMapper mapper, ILogger<BlogPostService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<BlogPost> ApplyFilter(IQueryable<BlogPost> query, BlogPostSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Title))
                query = query.Where(x => x.Title.Contains(search.Title));

            if (search.Status.HasValue)
                query = query.Where(x => x.Status == search.Status.Value);

            if (search.CreatedById.HasValue)
                query = query.Where(x => x.CreatedById == search.CreatedById.Value);

            if (search.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == search.ProjectId.Value);

            return query;
        }


        protected override async Task BeforeInsertAsync(BlogPost entity, BlogPostInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(BlogPost entity, BlogPostUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        public async Task<bool> IncrementViewCountAsync(Guid blogPostId, CancellationToken cancellationToken = default)
        {
            try
            {
                var blogPost = await _context.BlogPosts.FindAsync(new object[] { blogPostId }, cancellationToken);
                if (blogPost == null)
                {
                    _logger.LogWarning("Blog post not found for view count increment: {Id}", blogPostId);
                    return false;
                }

                blogPost.ViewCount++;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("View count incremented for blog post: {Id}, new count: {ViewCount}",
                    blogPostId, blogPost.ViewCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing view count for blog post: {Id}", blogPostId);
                return false;
            }
        }
    }
}
