namespace BookManager.Models.DTOs.Responses
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = null!;
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
