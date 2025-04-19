using System.Collections.Generic;

namespace Ecommerce.Core.DTOs
{

    public class PaginationRequestDto
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;
        private const int MaxPageSize = 50;
        
        public int PageNumber 
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }
        
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
        }
        
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDesc { get; set; } = true;
        public string SearchTerm { get; set; } = "";
    }
    

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
} 
