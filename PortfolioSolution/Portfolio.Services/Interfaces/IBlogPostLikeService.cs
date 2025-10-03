using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.BaseInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Interfaces
{
    public interface IBlogPostLikeService
        : ICRUDService<BlogPostLikeResponse, BlogPostLikeSearchObject, BlogPostLikeInsertRequest, object, Guid>
    {
        Task<BlogPostLikeStatusResponse> ToggleLikeAsync(Guid blogPostId, string visitorKey, CancellationToken cancellationToken = default);
        Task<BlogPostLikeStatusResponse> GetLikeStatusAsync(Guid blogPostId, string visitorKey, CancellationToken cancellationToken = default);
    }
}
