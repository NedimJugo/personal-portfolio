using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
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
    public interface IMediaService
        : ICRUDService<MediaResponse, MediaSearchObject, MediaInsertRequest, MediaUpdateRequest, Guid>
    {
        /// <summary>
        /// Returns the stored thumbnail bytes and mime type for a media record, or null if none exists.
        /// </summary>
        Task<(byte[] Data, string MimeType)?> GetThumbnailAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
