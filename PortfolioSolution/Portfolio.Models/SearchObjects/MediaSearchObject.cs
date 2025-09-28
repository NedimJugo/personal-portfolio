using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Models.SearchObjects
{
    public class MediaSearchObject : BaseSearchObject
    {
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? StorageProvider { get; set; }
        public int? UploadedById { get; set; }
    }
}
