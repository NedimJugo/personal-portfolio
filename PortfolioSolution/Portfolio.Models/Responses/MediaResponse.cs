using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.Responses
{
    public class MediaResponse
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string StorageProvider { get; set; } = "Local";
        public string FileType { get; set; } = "image";
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? AltText { get; set; }
        public string? Caption { get; set; }
        public string? Folder { get; set; }
        public int UploadedById { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
    }
}
