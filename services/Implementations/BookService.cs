using BookManager.db;
using BookManager.Exceptions;
using BookManager.Models.Domain;
using BookManager.Models.DTOs.Requests;
using BookManager.Models.DTOs.Responses;
using BookManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly BookDb _context;
        private readonly ILogger<BookService> _logger;

        public BookService(BookDb context, ILogger<BookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResponse<BookTitleDto>> GetPopularBooksAsync(
            int page,
            int pageSize
        )
        {
            var currentYear = DateTime.UtcNow.Year;

            var query = _context
                .Books.Where(b => !b.IsDeleted)
                .Select(b => new BookTitleDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    PopularityScore = (b.BookViews * 0.5) + ((currentYear - b.PublicationYear) * 2),
                })
                .OrderByDescending(b => b.PopularityScore);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResponse<BookTitleDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<BookDetailDto> GetBookAsync(int id)
        {
            var book = await GetBookByIdOrThrow(id);

            book.BookViews++;
            await _context.SaveChangesAsync();

            return CreateBookDetailDto(book);
        }

        public async Task<BookDetailDto> CreateBookAsync(CreateBookRequest request)
        {
            var exists = await _context.Books.AnyAsync(b =>
                b.Title.ToLower() == request.Title.ToLower() && !b.IsDeleted
            );

            if (exists)
                throw new DuplicateTitleException(request.Title);

            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                PublicationYear = request.PublicationYear,
                BookViews = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreateBookDetailDto(book);
        }

        public async Task<BulkCreateResponse> CreateBooksAsync(
            IEnumerable<CreateBookRequest> requests
        )
        {
            if (!requests.Any())
            {
                throw new ArgumentException("No books provided for creation.");
            }

            var requestedTitles = requests.Select(r => r.Title.ToLower()).ToList();
            var existingBooks = await _context
                .Books.Where(b => !b.IsDeleted && requestedTitles.Contains(b.Title.ToLower()))
                .ToListAsync();

            if (existingBooks.Any())
            {
                var duplicateInfo = requests
                    .Where(request =>
                        existingBooks.Any(existing =>
                            existing.Title.Equals(request.Title, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    .Select(request => new DuplicateBookInfo
                    {
                        AttemptedTitle = request.Title,
                        ExistingBook = CreateBookDetailDto(
                            existingBooks.First(b =>
                                b.Title.Equals(request.Title, StringComparison.OrdinalIgnoreCase)
                            )
                        ),
                    })
                    .ToList();

                if (duplicateInfo.Count == requests.Count())
                {
                    throw new DuplicateTitleException(
                        "All provided books already exist in the database.",
                        duplicateInfo
                    );
                }

                var newBooks = requests
                    .Where(request =>
                        !existingBooks.Any(existing =>
                            existing.Title.Equals(request.Title, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    .Select(request => new Book
                    {
                        Title = request.Title,
                        Author = request.Author,
                        PublicationYear = request.PublicationYear,
                        BookViews = 0,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                    })
                    .ToList();

                await _context.Books.AddRangeAsync(newBooks);
                await _context.SaveChangesAsync();

                return new BulkCreateResponse
                {
                    CreatedBooks = newBooks.Select(CreateBookDetailDto),
                    DuplicateBooks = duplicateInfo,
                };
            }

            var booksToCreate = requests
                .Select(request => new Book
                {
                    Title = request.Title,
                    Author = request.Author,
                    PublicationYear = request.PublicationYear,
                    BookViews = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                })
                .ToList();

            await _context.Books.AddRangeAsync(booksToCreate);
            await _context.SaveChangesAsync();

            return new BulkCreateResponse
            {
                CreatedBooks = booksToCreate.Select(CreateBookDetailDto),
                DuplicateBooks = new List<DuplicateBookInfo>(),
            };
        }

        public async Task UpdateBookAsync(UpdateBookRequest request, int id)
        {
            var book = await GetBookByIdOrThrow(id);
            bool hasChanges = false;

            if (!string.IsNullOrEmpty(request.Title) && book.Title != request.Title)
            {
                // Check for duplicate title if title is being changed
                var titleExists = await _context.Books.AnyAsync(b =>
                    b.Id != id && b.Title.ToLower() == request.Title.ToLower() && !b.IsDeleted
                );

                if (titleExists)
                    throw new DuplicateTitleException(request.Title);

                book.Title = request.Title;
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(request.Author) && book.Author != request.Author)
            {
                book.Author = request.Author;
                hasChanges = true;
            }

            if (book.PublicationYear != request.PublicationYear)
            {
                book.PublicationYear = request.PublicationYear;
                hasChanges = true;
            }

            if (!hasChanges)
            {
                throw new NoChangesException("No changes were made to the book.");
            }

            book.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await GetBookByIdOrThrow(id);
            book.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<(List<int> DeletedIds, List<int> NotFoundIds)> DeleteBooksAsync(
            IEnumerable<int> ids
        )
        {
            var books = await _context
                .Books.Where(b => ids.Contains(b.Id) && !b.IsDeleted)
                .ToListAsync();

            var foundIds = books.Select(b => b.Id).ToHashSet();
            var notFoundIds = ids.Except(foundIds).ToList();

            foreach (var book in books)
            {
                book.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return (foundIds.ToList(), notFoundIds);
        }

        private async Task<Book> GetBookByIdOrThrow(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (book == null)
                throw new BookNotFoundException(id);

            return book;
        }

        private BookDetailDto CreateBookDetailDto(Book book)
        {
            var currentYear = DateTime.UtcNow.Year;
            return new BookDetailDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                PublicationYear = book.PublicationYear,
                BookViews = book.BookViews,
                PopularityScore =
                    (book.BookViews * 0.5) + ((currentYear - book.PublicationYear) * 2),
            };
        }
    }
}
