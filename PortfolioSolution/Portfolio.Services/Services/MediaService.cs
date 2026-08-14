using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Models.Configuration;
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
    /// <summary>
    /// Media service with Azure Blob Storage integration.
    /// Automatically manages file URLs and handles blob cleanup on deletion.
    /// </summary>
    public class MediaService
        : BaseCRUDService<MediaResponse, MediaSearchObject, Media, MediaInsertRequest, MediaUpdateRequest, Guid>,
          IMediaService
    {
        private readonly IAzureBlobStorageService _blobStorageService;
        private readonly string _publicBaseUrl;

        public MediaService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<MediaService> logger,
            IAzureBlobStorageService blobStorageService,
            IOptions<AppSettings> appSettings)
            : base(context, mapper, logger)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _publicBaseUrl = appSettings?.Value?.PublicBaseUrl?.TrimEnd('/')
                ?? throw new ArgumentNullException(nameof(appSettings));
        }

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

        /// <summary>
        /// Before inserting a media record, generate the file URL from Azure Blob Storage.
        /// If the FileName is set and StorageProvider is "Azure", generate the URL.
        /// </summary>
        protected override async Task BeforeInsertAsync(
            Media entity,
            MediaInsertRequest request,
            CancellationToken cancellationToken = default)
        {
            entity.UploadedAt = DateTimeOffset.UtcNow;
            if (request.FileData != null && request.FileData.Length > 0)
            {
                entity.FileData = request.FileData;
                entity.StorageProvider = "Database";
            }

            if (string.IsNullOrWhiteSpace(entity.FileUrl) || entity.FileUrl.Contains("blob.core.windows.net"))
            {
                entity.FileUrl = $"https://portfolio-backend-jsyz.onrender.com/api/media/{entity.Id}/download";
            }

            // Fallback for Azure storage if specified
            if (entity.StorageProvider == "Azure" && !string.IsNullOrWhiteSpace(entity.FileName))
            {
                try
                {
                    entity.FileUrl = await _blobStorageService.GetFileUrlAsync(entity.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to generate Azure file URL for {FileName}. Using local download URL.",
                        entity.FileName);
                    entity.FileUrl = $"{_publicBaseUrl}/api/media/{entity.Id}/download";
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Before updating a media record, regenerate the file URL if FileName changed.
        /// This is useful if you're moving files or changing storage providers.
        /// </summary>
        protected override async Task BeforeUpdateAsync(
            Media entity,
            MediaUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            // If FileName or StorageProvider changed and using Azure, regenerate URL
            var fileNameChanged = !string.IsNullOrWhiteSpace(request.FileName)
                && request.FileName != entity.FileName;

            var storageProviderChanged = !string.IsNullOrWhiteSpace(request.StorageProvider)
                && request.StorageProvider != entity.StorageProvider;

            if ((fileNameChanged || storageProviderChanged) &&
                (request.StorageProvider == "Azure" || entity.StorageProvider == "Azure"))
            {
                // Get the new filename (either from request or existing entity)
                var fileName = !string.IsNullOrWhiteSpace(request.FileName)
                    ? request.FileName
                    : entity.FileName;

                // Get the new storage provider
                var storageProvider = !string.IsNullOrWhiteSpace(request.StorageProvider)
                    ? request.StorageProvider
                    : entity.StorageProvider;

                if (storageProvider == "Azure" && !string.IsNullOrWhiteSpace(fileName))
                {
                    try
                    {
                        var newFileUrl = await _blobStorageService.GetFileUrlAsync(fileName);

                        // Update the entity's FileUrl (this will be saved by the base service)
                        entity.FileUrl = newFileUrl;

                        _logger.LogInformation(
                            "Regenerated Azure Blob URL for media {Id}: {FileName} -> {FileUrl}",
                            entity.Id,
                            fileName,
                            newFileUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to regenerate file URL for {FileName}",
                            fileName);
                    }
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// After deleting a media record, delete the associated blob from Azure Storage.
        /// This ensures orphaned files don't remain in blob storage.
        /// </summary>
        protected override async Task AfterDeleteAsync(
            Media entity,
            CancellationToken cancellationToken = default)
        {
            // Only delete from Azure if StorageProvider is Azure
            if (entity.StorageProvider == "Azure" && !string.IsNullOrWhiteSpace(entity.FileName))
            {
                try
                {
                    _logger.LogInformation(
                        "Attempting to delete blob for media {Id}: {FileName}",
                        entity.Id,
                        entity.FileName);

                    var deleted = await _blobStorageService.DeleteFileAsync(entity.FileName, cancellationToken);

                    if (deleted)
                    {
                        _logger.LogInformation(
                            "Successfully deleted blob: {FileName}",
                            entity.FileName);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Blob not found in storage (may have been deleted already): {FileName}",
                            entity.FileName);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't throw - the database record is already deleted
                    // You may want to implement a cleanup job to handle orphaned blobs
                    _logger.LogError(ex,
                        "Failed to delete blob for media {Id}: {FileName}. Blob may be orphaned.",
                        entity.Id,
                        entity.FileName);
                }
            }

            await Task.CompletedTask;
        }
    }
}