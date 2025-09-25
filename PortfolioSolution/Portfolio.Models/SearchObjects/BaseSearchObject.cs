using System.ComponentModel.DataAnnotations;

namespace EcoChallenge.Models.SearchObjects
{
    public class BaseSearchObject
    {
        private int? _page = 0;
        private int? _pageSize = 20;
        private string _sortBy = "Id";

        [Range(0, int.MaxValue, ErrorMessage = "Page must be 0 or greater")]
        public int? Page
        {
            get => _page;
            set => _page = value >= 0 ? value : 0;
        }

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int? PageSize
        {
            get => _pageSize;
            set => _pageSize = value is >= 1 and <= 100 ? value : 20;
        }

        public string SortBy
        {
            get => _sortBy;
            set => _sortBy = !string.IsNullOrWhiteSpace(value) ? value : "Id";
        }

        public bool Desc { get; set; } = false;
        public bool IncludeTotalCount { get; set; } = true;
        public bool RetrieveAll { get; set; } = false;

        // Helper method to validate sort field against allowed fields
        public virtual bool IsValidSortField(string field)
        {
            // Override in derived classes to specify allowed sort fields
            return !string.IsNullOrWhiteSpace(field);
        }
    }
}
