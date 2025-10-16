using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Models.Configuration;
using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Portfolio.Services.Services
{
    /// <summary>
    /// Implementation of Azure Blob Storage service using Azure.Storage.Blobs SDK v12+.
    /// Provides production-ready file storage operations with streaming, SAS tokens, and error handling.
    /// </summary>
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly AzureStorageSettings _settings;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(
            IOptions<AzureStorageSettings> settings,
            ILogger<AzureBlobStorageService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Validate configuration
            if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage ConnectionString is not configured. " +
                    "Please set AzureStorageSettings:ConnectionString in appsettings.json or environment variables.");
            }

            if (string.IsNullOrWhiteSpace(_settings.ContainerName))
            {
                throw new InvalidOperationException("Container name must be specified in AzureStorageSettings.");
            }

            try
            {
                // Initialize blob service client
                _blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
                _containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);

                _logger.LogInformation(
                    "Azure Blob Storage service initialized for container: {ContainerName}",
                    _settings.ContainerName);

                // Create container if configured to do so
                if (_settings.CreateContainerIfNotExists)
                {
                    InitializeContainerAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure Blob Storage service");
                throw new InvalidOperationException(
                    "Failed to initialize Azure Blob Storage. Check connection string and network connectivity.", ex);
            }
        }

        /// <summary>
        /// Initializes the container, creating it if it doesn't exist.
        /// For production: Use private access and SAS tokens.
        /// For development: Can use BlobContainerPublicAccessType.Blob for easier testing.
        /// </summary>
        private async Task InitializeContainerAsync()
        {
            try
            {
                // Check if container exists
                var exists = await _containerClient.ExistsAsync();

                if (!exists)
                {
                    _logger.LogInformation("Creating blob container: {ContainerName}", _settings.ContainerName);

                    // Create container with private access (recommended for production)
                    // Use PublicAccessType.None for private access (requires SAS tokens)
                    // Use PublicAccessType.Blob for public read access to blobs (not recommended for production)
                    var accessType = _settings.UseSasTokens
                        ? PublicAccessType.None  // Private - requires SAS tokens
                        : PublicAccessType.Blob;  // Public blob access - direct URLs work

                    await _containerClient.CreateAsync(accessType);

                    _logger.LogInformation(
                        "Container created successfully with access type: {AccessType}",
                        accessType);
                }
                else
                {
                    _logger.LogDebug("Container already exists: {ContainerName}", _settings.ContainerName);
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                // Container already exists - this is fine
                _logger.LogDebug("Container already exists: {ContainerName}", _settings.ContainerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing container: {ContainerName}", _settings.ContainerName);
                throw;
            }
        }

        public async Task<string> UploadFileAsync(
            IFormFile file,
            string folder = "",
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty", nameof(file));
            }

            // Validate file size
            if (file.Length > _settings.MaxFileSizeBytes)
            {
                throw new InvalidOperationException(
                    $"File size ({file.Length} bytes) exceeds maximum allowed size ({_settings.MaxFileSizeBytes} bytes)");
            }

            // Validate file extension if restrictions are configured
            if (!string.IsNullOrWhiteSpace(_settings.AllowedExtensions))
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = _settings.AllowedExtensions
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim().ToLowerInvariant())
                    .ToList();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        $"File extension '{extension}' is not allowed. Allowed extensions: {_settings.AllowedExtensions}");
                }
            }

            try
            {
                // Open file stream
                using var stream = file.OpenReadStream();

                // Upload using the stream overload
                return await UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    folder,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder = "",
            CancellationToken cancellationToken = default)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty", nameof(fileName));
            }

            try
            {
                // Generate unique filename: GUID + original extension
                var extension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";

                // Add folder prefix if provided
                var blobName = string.IsNullOrWhiteSpace(folder)
                    ? uniqueFileName
                    : $"{folder.TrimEnd('/')}/{uniqueFileName}";

                _logger.LogInformation("Uploading file to blob: {BlobName}", blobName);

                // Get blob client
                var blobClient = _containerClient.GetBlobClient(blobName);

                // Set content type and other metadata
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType ?? "application/octet-stream"
                };

                var metadata = new Dictionary<string, string>
                {
                    { "OriginalFileName", fileName },
                    { "UploadedAt", DateTimeOffset.UtcNow.ToString("o") }
                };

                // Upload the stream (this streams data, doesn't load entire file into memory)
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Metadata = metadata,
                    // Optional: Set transfer options for performance tuning
                    TransferOptions = new Azure.Storage.StorageTransferOptions
                    {
                        MaximumConcurrency = 4,
                        MaximumTransferSize = 4 * 1024 * 1024 // 4 MB
                    }
                };

                await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);

                _logger.LogInformation(
                    "File uploaded successfully: {BlobName} (Original: {OriginalFileName})",
                    blobName,
                    fileName);

                return blobName;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex,
                    "Azure Storage request failed while uploading file: {FileName}. Status: {Status}, Error: {ErrorCode}",
                    fileName, ex.Status, ex.ErrorCode);
                throw new InvalidOperationException($"Failed to upload file to Azure Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading file: {FileName}", fileName);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty", nameof(fileName));
            }

            try
            {
                _logger.LogDebug("Downloading file: {FileName}", fileName);

                var blobClient = _containerClient.GetBlobClient(fileName);

                // Check if blob exists
                var exists = await blobClient.ExistsAsync(cancellationToken);
                if (!exists)
                {
                    _logger.LogWarning("File not found: {FileName}", fileName);
                    throw new FileNotFoundException($"File not found: {fileName}");
                }

                // Download to stream (streams data, doesn't load entire file into memory)
                var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

                _logger.LogInformation("File downloaded successfully: {FileName}", fileName);

                // Return the stream - caller is responsible for disposing
                return response.Value.Content;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning("File not found in blob storage: {FileName}", fileName);
                throw new FileNotFoundException($"File not found: {fileName}", ex);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex,
                    "Azure Storage request failed while downloading file: {FileName}. Status: {Status}, Error: {ErrorCode}",
                    fileName, ex.Status, ex.ErrorCode);
                throw new InvalidOperationException($"Failed to download file from Azure Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error downloading file: {FileName}", fileName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty", nameof(fileName));
            }

            try
            {
                _logger.LogInformation("Deleting file: {FileName}", fileName);

                var blobClient = _containerClient.GetBlobClient(fileName);
                var response = await blobClient.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.IncludeSnapshots,
                    cancellationToken: cancellationToken);

                if (response.Value)
                {
                    _logger.LogInformation("File deleted successfully: {FileName}", fileName);
                    return true;
                }
                else
                {
                    _logger.LogWarning("File not found for deletion: {FileName}", fileName);
                    return false;
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex,
                    "Azure Storage request failed while deleting file: {FileName}. Status: {Status}, Error: {ErrorCode}",
                    fileName, ex.Status, ex.ErrorCode);
                throw new InvalidOperationException($"Failed to delete file from Azure Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting file: {FileName}", fileName);
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);
                var exists = await blobClient.ExistsAsync(cancellationToken);
                return exists.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if file exists: {FileName}", fileName);
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty", nameof(fileName));
            }

            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);

                // If using SAS tokens, generate a SAS URL with default expiry
                if (_settings.UseSasTokens)
                {
                    var expiry = TimeSpan.FromHours(_settings.DefaultSasExpiryHours);
                    return await GenerateSasUrlAsync(fileName, expiry);
                }

                // Otherwise, return the direct blob URL (requires public container access)
                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating file URL: {FileName}", fileName);
                throw;
            }
        }

        public async Task<string> GenerateSasUrlAsync(string fileName, TimeSpan expiry)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty", nameof(fileName));
            }

            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);

                // Check if blob exists
                var exists = await blobClient.ExistsAsync();
                if (!exists)
                {
                    throw new FileNotFoundException($"File not found: {fileName}");
                }

                // Check if we can generate SAS tokens (requires account key)
                if (!blobClient.CanGenerateSasUri)
                {
                    _logger.LogWarning(
                        "Cannot generate SAS token for blob. " +
                        "Ensure you're using connection string with account key, not SAS token connection.");

                    // Fallback to direct URL
                    return blobClient.Uri.AbsoluteUri;
                }

                // Create SAS builder
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _settings.ContainerName,
                    BlobName = fileName,
                    Resource = "b", // b = blob
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Grace period for clock skew
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
                };

                // Set permissions (read only for file access)
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Generate the SAS URI
                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                _logger.LogDebug(
                    "SAS URL generated for {FileName}, expires in {ExpiryHours} hours",
                    fileName,
                    expiry.TotalHours);

                return sasUri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS URL for file: {FileName}", fileName);
                throw;
            }
        }

        public async Task<IEnumerable<string>> ListFilesAsync(
            string folder = "",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var files = new List<string>();

                // Set prefix if folder is specified
                var prefix = string.IsNullOrWhiteSpace(folder)
                    ? null
                    : folder.TrimEnd('/') + "/";

                _logger.LogDebug("Listing files with prefix: {Prefix}", prefix ?? "(none)");

                // List blobs with optional prefix
                await foreach (var blobItem in _containerClient.GetBlobsAsync(
                    prefix: prefix,
                    cancellationToken: cancellationToken))
                {
                    files.Add(blobItem.Name);
                }

                _logger.LogInformation("Listed {Count} files", files.Count);
                return files;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex,
                    "Azure Storage request failed while listing files. Status: {Status}, Error: {ErrorCode}",
                    ex.Status, ex.ErrorCode);
                throw new InvalidOperationException($"Failed to list files in Azure Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error listing files");
                throw;
            }
        }

        public async Task<bool> CopyFileAsync(
            string sourceFileName,
            string destinationFileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new ArgumentException("Source file name cannot be empty", nameof(sourceFileName));
            }

            if (string.IsNullOrWhiteSpace(destinationFileName))
            {
                throw new ArgumentException("Destination file name cannot be empty", nameof(destinationFileName));
            }

            try
            {
                _logger.LogInformation(
                    "Copying file from {Source} to {Destination}",
                    sourceFileName,
                    destinationFileName);

                var sourceBlobClient = _containerClient.GetBlobClient(sourceFileName);
                var destinationBlobClient = _containerClient.GetBlobClient(destinationFileName);

                // Check if source exists
                var exists = await sourceBlobClient.ExistsAsync(cancellationToken);
                if (!exists)
                {
                    _logger.LogWarning("Source file not found: {SourceFileName}", sourceFileName);
                    return false;
                }

                // Start copy operation (this is async on Azure's side)
                var copyOperation = await destinationBlobClient.StartCopyFromUriAsync(
                    sourceBlobClient.Uri,
                    cancellationToken: cancellationToken);

                // Wait for copy to complete
                await copyOperation.WaitForCompletionAsync(cancellationToken);

                if (copyOperation.HasCompleted && !copyOperation.HasValue)
                {
                    _logger.LogError("Copy operation completed but no value returned");
                    return false;
                }

                _logger.LogInformation(
                    "File copied successfully from {Source} to {Destination}",
                    sourceFileName,
                    destinationFileName);

                return true;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex,
                    "Azure Storage request failed while copying file. Status: {Status}, Error: {ErrorCode}",
                    ex.Status, ex.ErrorCode);
                throw new InvalidOperationException($"Failed to copy file in Azure Storage: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error copying file from {Source} to {Destination}",
                    sourceFileName,
                    destinationFileName);
                throw;
            }
        }
    }
}