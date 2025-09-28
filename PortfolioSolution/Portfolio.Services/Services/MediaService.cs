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
    public class MediaService
        : BaseCRUDService<MediaResponse, MediaSearchObject, Media, MediaInsertRequest, MediaUpdateRequest, Guid>,
          IMediaService
    {
        public MediaService(ApplicationDbContext context, IMapper mapper, ILogger<MediaService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Media> ApplyFilter(IQueryable<Media> query, MediaSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.FileName))
                query = query.Where(x => x.FileName.Contains(search.FileName));

            if (!string.IsNullOrWhiteSpace(search.FileType))
                query = query.Where(x => x.FileType == search.FileType);

            if (!string.IsNullOrWhiteSpace(search.StorageProvider))
                query = query.Where(x => x.StorageProvider == search.StorageProvider);

            if (search.UploadedById.HasValue)
                query = query.Where(x => x.UploadedById == search.UploadedById.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(Media entity, MediaInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.UploadedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Media entity, MediaUpdateRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}
