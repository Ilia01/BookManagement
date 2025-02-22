using BookManager.Models.Domain;
using BookManager.Models.DTOs.Requests;
using BookManager.Models.DTOs.Responses;

namespace BookManager.Services.Interfaces
{
    public interface IBookService
    {
        Task<PaginatedResponse<BookTitleDto>> GetPopularBooksAsync(int page, int pageSize);
        Task<BookDetailDto> GetBookAsync(int id);
        Task<BookDetailDto> CreateBookAsync(CreateBookRequest request);
        Task<BulkCreateResponse> CreateBooksAsync(IEnumerable<CreateBookRequest> requests);
        Task UpdateBookAsync(UpdateBookRequest request, int id);
        Task DeleteBookAsync(int id);
        Task<(List<int> DeletedIds, List<int> NotFoundIds)> DeleteBooksAsync(IEnumerable<int> ids);
    }
}
