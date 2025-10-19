using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.Interfaces;
using Portfolio.WebAPI.BaseContoller;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Portfolio.WebAPI.Controllers
{
    /// <summary>
    /// Controller for media management with Azure Blob Storage integration.
    /// Supports file upload, download, and management operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController
        : BaseCRUDController<MediaResponse, MediaSearchObject, MediaInsertRequest, MediaUpdateRequest, Guid>
    {
        private readonly IAzureBlobStorageService _blobStorageService;
        private readonly IMediaService _mediaService;

        public MediaController(
            IMediaService service,
            ILogger<MediaController> logger,
            IAzureBlobStorageService blobStorageService)
            : base(service, logger)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _mediaService = service;
        }

        /// <summary>
        /// Uploads a single file to Azure Blob Storage and creates a media record.
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="altText">Optional alt text for accessibility</param>
        /// <param name="caption">Optional caption</param>
        /// <param name="folder">Optional folder/category (e.g., "projects", "blog", "avatars")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created media record with file URL</returns>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(MediaResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(413)] // Payload too large
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit at controller level
        public async Task<ActionResult<MediaResponse>> UploadFile(
            IFormFile file,
            [FromForm] string? altText = null,
            [FromForm] string? caption = null,
            [FromForm] string? folder = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided or file is empty");
                }

                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized("User ID not found in claims");
                }

                _logger.LogInformation(
                    "User {UserId} uploading file: {FileName} ({Size} bytes)",
                    userId.Value,
                    file.FileName,
                    file.Length);

                // Upload to Azure Blob Storage
                var blobName = await _blobStorageService.UploadFileAsync(
                    file,
                    folder ?? "uploads",
                    cancellationToken);

                // Get the file URL
                var fileUrl = await _blobStorageService.GetFileUrlAsync(blobName);

                // Determine file type from content type
                var fileType = DetermineFileType(file.ContentType);

                // Extract image dimensions if it's an image
                int? width = null;
                int? height = null;
                if (fileType == "image")
                {
                    try
                    {
                        using var imageStream = file.OpenReadStream();
                        using var image = await Image.LoadAsync(imageStream, cancellationToken);
                        width = image.Width;
                        height = image.Height;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to extract image dimensions for {FileName}", file.FileName);
                    }
                }

                // Create media record in database
                var mediaRequest = new MediaInsertRequest
                {
                    FileName = blobName,
                    OriginalFileName = file.FileName,
                    FileUrl = fileUrl,
                    StorageProvider = "Azure",
                    FileType = fileType,
                    FileSize = file.Length,
                    MimeType = file.ContentType ?? "application/octet-stream",
                    Width = width,
                    Height = height,
                    AltText = altText,
                    Caption = caption,
                    Folder = folder,
                    UploadedById = userId.Value
                };

                var result = await _mediaService.CreateAsync(mediaRequest, cancellationToken);

                _logger.LogInformation(
                    "File uploaded successfully: {BlobName} -> Media ID: {MediaId}",
                    blobName,
                    result.Id);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Upload validation failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
                return StatusCode(500, "An error occurred while uploading the file");
            }
        }

        /// <summary>
        /// Uploads multiple files to Azure Blob Storage and creates media records.
        /// </summary>
        /// <param name="files">The files to upload</param>
        /// <param name="folder">Optional folder/category</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of created media records</returns>
        [HttpPost("upload/bulk")]
        [ProducesResponseType(typeof(MediaResponse[]), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB total limit for bulk upload
        public async Task<ActionResult<MediaResponse[]>> UploadMultipleFiles(
            [FromForm] IFormFileCollection files,
            [FromForm] string? folder = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files provided");
                }

                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized("User ID not found in claims");
                }

                _logger.LogInformation(
                    "User {UserId} uploading {Count} files",
                    userId.Value,
                    files.Count);

                var results = new List<MediaResponse>();

                foreach (var file in files)
                {
                    if (file == null || file.Length == 0)
                    {
                        _logger.LogWarning("Skipping empty file in bulk upload");
                        continue;
                    }

                    try
                    {
                        // Upload to Azure Blob Storage
                        var blobName = await _blobStorageService.UploadFileAsync(
                            file,
                            folder ?? "uploads",
                            cancellationToken);

                        // Get the file URL
                        var fileUrl = await _blobStorageService.GetFileUrlAsync(blobName);

                        // Determine file type
                        var fileType = DetermineFileType(file.ContentType);

                        // Extract image dimensions if it's an image
                        int? width = null;
                        int? height = null;
                        if (fileType == "image")
                        {
                            try
                            {
                                using var imageStream = file.OpenReadStream();
                                using var image = await Image.LoadAsync(imageStream, cancellationToken);
                                width = image.Width;
                                height = image.Height;
                            }
                            catch { /* Ignore dimension extraction errors */ }
                        }

                        // Create media record
                        var mediaRequest = new MediaInsertRequest
                        {
                            FileName = blobName,
                            OriginalFileName = file.FileName,
                            FileUrl = fileUrl,
                            StorageProvider = "Azure",
                            FileType = fileType,
                            FileSize = file.Length,
                            MimeType = file.ContentType ?? "application/octet-stream",
                            Width = width,
                            Height = height,
                            Folder = folder,
                            UploadedById = userId.Value
                        };

                        var result = await _mediaService.CreateAsync(mediaRequest, cancellationToken);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading file in bulk: {FileName}", file.FileName);
                        // Continue with other files
                    }
                }

                _logger.LogInformation(
                    "Bulk upload completed: {Successful}/{Total} files",
                    results.Count,
                    files.Count);

                return CreatedAtAction(nameof(Get), results.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk file upload");
                return StatusCode(500, "An error occurred while uploading files");
            }
        }

        /// <summary>
        /// Downloads a file from Azure Blob Storage by media ID.
        /// </summary>
        /// <param name="id">Media record ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File stream</returns>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get media record
                var media = await _mediaService.GetByIdAsync(id, cancellationToken);
                if (media == null)
                {
                    return NotFound($"Media with ID {id} not found");
                }

                // Only download from Azure storage
                if (media.StorageProvider != "Azure")
                {
                    return BadRequest($"Media is stored in {media.StorageProvider}, not Azure");
                }

                _logger.LogInformation("Downloading file for media {Id}: {FileName}", id, media.FileName);

                // Download from blob storage
                var stream = await _blobStorageService.DownloadFileAsync(media.FileName, cancellationToken);

                // Return file stream
                return File(stream, media.MimeType, media.OriginalFileName);
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("File not found in blob storage for media {Id}", id);
                return NotFound("File not found in storage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file for media {Id}", id);
                return StatusCode(500, "An error occurred while downloading the file");
            }
        }

        /// <summary>
        /// Downloads a file directly by blob name (bypasses database lookup).
        /// Useful for quick downloads when you have the blob name.
        /// </summary>
        /// <param name="fileName">Blob name (including folder prefix)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File stream</returns>
        [HttpGet("download/{*fileName}")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(404)]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFileByName(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return BadRequest("File name is required");
                }

                _logger.LogInformation("Downloading file by name: {FileName}", fileName);

                // Download from blob storage
                var stream = await _blobStorageService.DownloadFileAsync(fileName, cancellationToken);

                // Try to determine content type from extension
                var contentType = GetContentTypeFromExtension(fileName);

                return File(stream, contentType, Path.GetFileName(fileName));
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("File not found: {FileName}", fileName);
                return NotFound("File not found in storage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
                return StatusCode(500, "An error occurred while downloading the file");
            }
        }

        /// <summary>
        /// Generates a temporary SAS URL for a media file.
        /// Useful for providing time-limited access to private files.
        /// </summary>
        /// <param name="id">Media record ID</param>
        /// <param name="expiryHours">How many hours the URL should be valid (default: 1)</param>
        /// <returns>SAS URL</returns>
        [HttpGet("{id}/sas-url")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<object>> GenerateSasUrl(
            Guid id,
            [FromQuery] int expiryHours = 1)
        {
            try
            {
                if (expiryHours < 1 || expiryHours > 168) // Max 7 days
                {
                    return BadRequest("Expiry hours must be between 1 and 168 (7 days)");
                }

                // Get media record
                var media = await _mediaService.GetByIdAsync(id);
                if (media == null)
                {
                    return NotFound($"Media with ID {id} not found");
                }

                if (media.StorageProvider != "Azure")
                {
                    return BadRequest($"Media is not stored in Azure");
                }

                // Generate SAS URL
                var sasUrl = await _blobStorageService.GenerateSasUrlAsync(
                    media.FileName,
                    TimeSpan.FromHours(expiryHours));

                return Ok(new
                {
                    mediaId = media.Id,
                    fileName = media.FileName,
                    originalFileName = media.OriginalFileName,
                    sasUrl = sasUrl,
                    expiresAt = DateTimeOffset.UtcNow.AddHours(expiryHours)
                });
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found in storage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS URL for media {Id}", id);
                return StatusCode(500, "An error occurred while generating SAS URL");
            }
        }

        /// <summary>
        /// Lists all files in a specific folder.
        /// </summary>
        /// <param name="folder">Folder name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of blob names</returns>
        [HttpGet("list/{folder?}")]
        [ProducesResponseType(typeof(string[]), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<string[]>> ListFiles(
            string? folder = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var files = await _blobStorageService.ListFilesAsync(folder ?? "", cancellationToken);
                return Ok(files.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files in folder: {Folder}", folder);
                return StatusCode(500, "An error occurred while listing files");
            }
        }

        #region Helper Methods

        /// <summary>
        /// Determines file type category from MIME type.
        /// </summary>
        private static string DetermineFileType(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return "document";

            contentType = contentType.ToLowerInvariant();

            if (contentType.StartsWith("image/"))
                return "image";
            if (contentType.StartsWith("video/"))
                return "video";
            if (contentType.StartsWith("audio/"))
                return "audio";

            return "document";
        }

        /// <summary>
        /// Gets content type from file extension.
        /// </summary>
        private static string GetContentTypeFromExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                _ => "application/octet-stream"
            };
        }
        /// <summary>
        /// Regenerates all media URLs to remove expired SAS tokens.
        /// Use this after changing from SAS tokens to public container access.
        /// </summary>
        [HttpPost("fix-urls")]
        [AllowAnonymous]  // Remove this after running once
        public async Task<IActionResult> FixMediaUrls(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting media URL fix...");

                // Get all Azure media records
                var allMedia = await _mediaService.GetAsync(new MediaSearchObject
                {
                    Page = 0,
                    PageSize = 10000,
                    RetrieveAll = true,
                    StorageProvider = "Azure"
                }, cancellationToken);

                int updated = 0;
                int errors = 0;

                foreach (var media in allMedia.Items)
                {
                    try
                    {
                        // Check if URL has SAS token (contains query parameters)
                        if (media.FileUrl.Contains("?"))
                        {
                            // Generate new public URL without SAS token
                            var newUrl = $"https://nedimjportfolioblob.blob.core.windows.net/potrfolioimagecontainer/{media.FileName}";

                            _logger.LogInformation(
                                "Updating media {Id}: {OldUrl} -> {NewUrl}",
                                media.Id,
                                media.FileUrl,
                                newUrl);

                            // Update directly in database
                            var entity = await _mediaService.GetByIdAsync(media.Id, cancellationToken);
                            if (entity != null)
                            {
                                // Create update request with new URL
                                var updateRequest = new MediaUpdateRequest
                                {
                                    FileName = media.FileName,
                                    OriginalFileName = media.OriginalFileName,
                                    FileUrl = newUrl,  // New URL without SAS
                                    StorageProvider = media.StorageProvider,
                                    FileType = media.FileType,
                                    FileSize = media.FileSize,
                                    MimeType = media.MimeType,
                                    Width = media.Width,
                                    Height = media.Height,
                                    AltText = media.AltText,
                                    Caption = media.Caption,
                                    Folder = media.Folder
                                };

                                await _mediaService.UpdateAsync(media.Id, updateRequest, cancellationToken);
                                updated++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating media {Id}", media.Id);
                        errors++;
                    }
                }

                _logger.LogInformation(
                    "Media URL fix completed: {Updated} updated, {Errors} errors",
                    updated,
                    errors);

                return Ok(new
                {
                    success = true,
                    updated = updated,
                    errors = errors,
                    total = allMedia.TotalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing media URLs");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        #endregion
    }
}