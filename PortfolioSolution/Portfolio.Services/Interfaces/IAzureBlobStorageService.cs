using Microsoft.AspNetCore.Http;

namespace Portfolio.Services.Interfaces
{
    /// <summary>
    /// Interface for Azure Blob Storage operations.
    /// Provides methods for uploading, downloading, deleting, and managing files in Azure Blob Storage.
    /// </summary>
    public interface IAzureBlobStorageService
    {
        /// <summary>
        /// Uploads a file from IFormFile to Azure Blob Storage.
        /// Generates a unique filename (GUID-based) and sets appropriate content type.
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="folder">Optional folder/prefix for organizing files (e.g., "images", "documents")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The blob name (including folder prefix if provided)</returns>
        Task<string> UploadFileAsync(
            IFormFile file,
            string folder = "",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a file from a stream to Azure Blob Storage.
        /// Useful for scenarios where you have file data but not an IFormFile.
        /// </summary>
        /// <param name="fileStream">The stream containing file data</param>
        /// <param name="fileName">The desired filename (extension will be preserved)</param>
        /// <param name="contentType">MIME type of the file</param>
        /// <param name="folder">Optional folder/prefix</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The blob name (including folder prefix if provided)</returns>
        Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder = "",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file from Azure Blob Storage as a stream.
        /// Caller is responsible for disposing the stream.
        /// </summary>
        /// <param name="fileName">The blob name to download (including folder prefix if applicable)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream containing the file data</returns>
        Task<Stream> DownloadFileAsync(
            string fileName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file from Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The blob name to delete (including folder prefix if applicable)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully, false if file not found</returns>
        Task<bool> DeleteFileAsync(
            string fileName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a file exists in Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The blob name to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if file exists, false otherwise</returns>
        Task<bool> FileExistsAsync(
            string fileName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the public URL for a blob.
        /// Returns either a direct blob URL (if container is public) or a SAS URL (if private).
        /// </summary>
        /// <param name="fileName">The blob name</param>
        /// <returns>The accessible URL for the blob</returns>
        Task<string> GetFileUrlAsync(string fileName);

        /// <summary>
        /// Generates a Shared Access Signature (SAS) URL for a blob with specified expiry.
        /// Useful for providing temporary access to private files.
        /// </summary>
        /// <param name="fileName">The blob name</param>
        /// <param name="expiry">How long the SAS URL should be valid</param>
        /// <returns>A SAS URL that expires after the specified duration</returns>
        Task<string> GenerateSasUrlAsync(string fileName, TimeSpan expiry);

        /// <summary>
        /// Lists all files in the container, optionally filtered by folder prefix.
        /// </summary>
        /// <param name="folder">Optional folder prefix to filter results</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of blob names</returns>
        Task<IEnumerable<string>> ListFilesAsync(
            string folder = "",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Copies a file within Azure Blob Storage.
        /// Useful for creating duplicates or moving files between folders.
        /// </summary>
        /// <param name="sourceFileName">Source blob name</param>
        /// <param name="destinationFileName">Destination blob name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if copied successfully</returns>
        Task<bool> CopyFileAsync(
            string sourceFileName,
            string destinationFileName,
            CancellationToken cancellationToken = default);
    }
}