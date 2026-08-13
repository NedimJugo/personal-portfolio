using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Services.Database;
using Portfolio.Services.Database.Entities;
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
    /// Database-backed storage service storing images directly in PostgreSQL.
    /// Eliminates external cloud dependencies (Azure Blob Storage).
    /// </summary>
    public class DatabaseBlobStorageService : IAzureBlobStorageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseBlobStorageService> _logger;

        public DatabaseBlobStorageService(
            ApplicationDbContext context,
            ILogger<DatabaseBlobStorageService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            using var stream = file.OpenReadStream();
            return await UploadFileAsync(stream, file.FileName, file.ContentType, folder, cancellationToken);
        }

        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder = "",
            CancellationToken cancellationToken = default)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var blobName = string.IsNullOrWhiteSpace(folder)
                ? uniqueFileName
                : $"{folder.TrimEnd('/')}/{uniqueFileName}";

            _logger.LogInformation("Database Storage uploading file: {BlobName}", blobName);

            // Read bytes from stream
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, cancellationToken);
            var bytes = ms.ToArray();

            // If a Media record already exists for this fileName, update its FileData
            var media = await _context.Media.FirstOrDefaultAsync(m => m.FileName == blobName, cancellationToken);
            if (media != null)
            {
                media.FileData = bytes;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return blobName;
        }

        public async Task<Stream> DownloadFileAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty", nameof(fileName));

            _logger.LogDebug("Database Storage downloading file: {FileName}", fileName);

            Media? media = await _context.Media.FirstOrDefaultAsync(m => m.FileName == fileName, cancellationToken);
            if (media == null && Guid.TryParse(fileName, out Guid mediaId))
            {
                media = await _context.Media.FirstOrDefaultAsync(m => m.Id == mediaId, cancellationToken);
            }
            if (media == null)
            {
                var baseName = Path.GetFileName(fileName);
                media = await _context.Media.FirstOrDefaultAsync(m => m.FileName.EndsWith(baseName), cancellationToken);
            }

            if (media == null || media.FileData == null || media.FileData.Length == 0)
            {
                _logger.LogWarning("File content not found in database for: {FileName}", fileName);
                throw new FileNotFoundException($"File not found in database storage: {fileName}");
            }

            return new MemoryStream(media.FileData);
        }

        public async Task<bool> DeleteFileAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var media = await _context.Media.FirstOrDefaultAsync(m => m.FileName == fileName, cancellationToken);
            if (media != null)
            {
                media.FileData = null;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<bool> FileExistsAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            return await _context.Media.AnyAsync(m => (m.FileName == fileName || m.Id.ToString() == fileName) && m.FileData != null, cancellationToken);
        }

        public Task<string> GetFileUrlAsync(string fileName)
        {
            var baseUrl = "https://portfolio-backend-jsyz.onrender.com";
            return Task.FromResult($"{baseUrl}/api/media/download/{fileName}");
        }

        public Task<string> GenerateSasUrlAsync(string fileName, TimeSpan expiry)
        {
            return GetFileUrlAsync(fileName);
        }

        public async Task<IEnumerable<string>> ListFilesAsync(
            string folder = "",
            CancellationToken cancellationToken = default)
        {
            var query = _context.Media.Where(m => m.FileData != null).AsQueryable();
            if (!string.IsNullOrWhiteSpace(folder))
            {
                var prefix = folder.TrimEnd('/') + "/";
                query = query.Where(m => m.FileName.StartsWith(prefix));
            }
            return await query.Select(m => m.FileName).ToListAsync(cancellationToken);
        }

        public async Task<bool> CopyFileAsync(
            string sourceFileName,
            string destinationFileName,
            CancellationToken cancellationToken = default)
        {
            var source = await _context.Media.FirstOrDefaultAsync(m => m.FileName == sourceFileName, cancellationToken);
            if (source == null || source.FileData == null) return false;

            var dest = await _context.Media.FirstOrDefaultAsync(m => m.FileName == destinationFileName, cancellationToken);
            if (dest != null)
            {
                dest.FileData = source.FileData;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
    }
}
