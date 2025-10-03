using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Models.Requests.InsertRequests;
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
    public class BlogPostLikeService
        : BaseCRUDService<BlogPostLikeResponse, BlogPostLikeSearchObject, BlogPostLike, BlogPostLikeInsertRequest, object, Guid>,
          IBlogPostLikeService
    {
        public BlogPostLikeService(ApplicationDbContext context, IMapper mapper, ILogger<BlogPostLikeService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<BlogPostLike> ApplyFilter(IQueryable<BlogPostLike> query, BlogPostLikeSearchObject? search = null)
        {
            if (search == null) return query;

            if (search.BlogPostId.HasValue)
                query = query.Where(x => x.BlogPostId == search.BlogPostId.Value);

            if (!string.IsNullOrWhiteSpace(search.VisitorKey))
                query = query.Where(x => x.VisitorKey == search.VisitorKey);
            return query;
        }

        public async Task<BlogPostLikeStatusResponse> ToggleLikeAsync(Guid blogPostId, string visitorKey, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var blogPost = await _context.Set<BlogPost>().FindAsync(new object[] { blogPostId }, cancellationToken);
                if (blogPost == null)
                {
                    throw new InvalidOperationException($"Blog post with ID {blogPostId} not found");
                }

                var existingLike = await _context.Set<BlogPostLike>()
                    .FirstOrDefaultAsync(x => x.BlogPostId == blogPostId && x.VisitorKey == visitorKey, cancellationToken);

                bool isLiked;
                if (existingLike != null)
                {
                    // Unlike
                    _context.Set<BlogPostLike>().Remove(existingLike);
                    blogPost.LikeCount = Math.Max(0, blogPost.LikeCount - 1);
                    isLiked = false;
                }
                else
                {
                    // Like
                    var newLike = new BlogPostLike
                    {
                        BlogPostId = blogPostId,
                        VisitorKey = visitorKey,
                        LikedAt = DateTimeOffset.UtcNow
                    };
                    _context.Set<BlogPostLike>().Add(newLike);
                    blogPost.LikeCount++;
                    isLiked = true;
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new BlogPostLikeStatusResponse
                {
                    IsLiked = isLiked,
                    TotalLikes = blogPost.LikeCount
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error toggling like for blog post {BlogPostId} by visitor {VisitorKey}", blogPostId, visitorKey);
                throw;
            }
        }

        public async Task<BlogPostLikeStatusResponse> GetLikeStatusAsync(Guid blogPostId, string visitorKey, CancellationToken cancellationToken = default)
        {
            var blogPost = await _context.Set<BlogPost>().FindAsync(new object[] { blogPostId }, cancellationToken);
            if (blogPost == null)
            {
                throw new InvalidOperationException($"Blog post with ID {blogPostId} not found");
            }

            var isLiked = await _context.Set<BlogPostLike>()
                .AnyAsync(x => x.BlogPostId == blogPostId && x.VisitorKey == visitorKey, cancellationToken);

            return new BlogPostLikeStatusResponse
            {
                IsLiked = isLiked,
                TotalLikes = blogPost.LikeCount
            };
        }

        protected override async Task BeforeInsertAsync(BlogPostLike entity, BlogPostLikeInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.LikedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
