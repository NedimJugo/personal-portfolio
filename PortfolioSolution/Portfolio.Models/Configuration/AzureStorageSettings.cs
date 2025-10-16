namespace Portfolio.Models.Configuration
{
    /// <summary>
    /// Configuration settings for Azure Blob Storage.
    /// Bind this from appsettings.json or environment variables.
    /// </summary>
    public class AzureStorageSettings
    {
        /// <summary>
        /// Azure Storage connection string.
        /// Format: DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net
        /// IMPORTANT: Never commit this to source control. Use User Secrets, Azure Key Vault, or environment variables.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Name of the blob container to use.
        /// Container names must be lowercase and between 3-63 characters.
        /// Example: "media", "portfolio-files", "uploads"
        /// </summary>
        public string ContainerName { get; set; } = "media";

        /// <summary>
        /// Base URL for accessing blobs (optional).
        /// If not provided, will be constructed from the connection string.
        /// Example: "https://yourstorageaccount.blob.core.windows.net"
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Whether to use SAS tokens for file URLs (recommended for production).
        /// If true, GetFileUrlAsync will return SAS URLs with default expiry.
        /// If false, returns direct blob URLs (requires public container access).
        /// </summary>
        public bool UseSasTokens { get; set; } = true;

        /// <summary>
        /// Default SAS token expiry duration in hours.
        /// Used when UseSasTokens is true and no explicit expiry is provided.
        /// Default: 24 hours
        /// </summary>
        public int DefaultSasExpiryHours { get; set; } = 24;

        /// <summary>
        /// Maximum file size allowed for upload in bytes.
        /// Default: 10 MB (10 * 1024 * 1024 bytes)
        /// Azure Blob Storage supports up to 5000 MB for block blobs in a single request.
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// Allowed file extensions (comma-separated, with dots).
        /// Empty means all extensions are allowed.
        /// Example: ".jpg,.jpeg,.png,.gif,.pdf,.docx"
        /// </summary>
        public string AllowedExtensions { get; set; } = string.Empty;

        /// <summary>
        /// Whether to create the container automatically if it doesn't exist.
        /// Recommended: true for development, false for production (pre-create containers).
        /// </summary>
        public bool CreateContainerIfNotExists { get; set; } = true;
    }
}