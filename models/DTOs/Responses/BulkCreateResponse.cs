namespace BookManager.Models.DTOs.Responses
{
    public class BulkCreateResponse
    {
        public IEnumerable<BookDetailDto> CreatedBooks { get; set; }
        public IEnumerable<DuplicateBookInfo> DuplicateBooks { get; set; }

        public BulkCreateResponse()
        {
            CreatedBooks = new List<BookDetailDto>();
            DuplicateBooks = new List<DuplicateBookInfo>();
        }
    }

    public class DuplicateBookInfo
    {
        public string AttemptedTitle { get; set; } = string.Empty;
        public BookDetailDto ExistingBook { get; set; } = new BookDetailDto();
    }
}
