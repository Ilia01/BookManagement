using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookManager.Exceptions;
using BookManager.Models;
using BookManager.Models.DTOs.Requests;
using BookManager.Models.DTOs.Responses;
using BookManager.Services;
using BookManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<BookTitleDto>>> GetBookTitles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(new ErrorResponse("Invalid pagination parameters"));

            try
            {
                var result = await _bookService.GetPopularBooksAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book titles");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("An error occurred while processing your request")
                );
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDetailDto>> GetBook(int id)
        {
            try
            {
                var book = await _bookService.GetBookAsync(id);
                return book == null ? NotFound() : Ok(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book with ID {BookId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("An error occurred while processing your request")
                );
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookDetailDto>> CreateBook(
            [FromBody] CreateBookRequest request
        )
        {
            try
            {
                var book = await _bookService.CreateBookAsync(request);
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (DuplicateTitleException)
            {
                return Conflict(new ErrorResponse("A book with the same title already exists"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("An error occurred while processing your request")
                );
            }
        }

        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BulkCreateResponse>> CreateBooks(
            [FromBody] IEnumerable<CreateBookRequest> requests
        )
        {
            if (requests == null || !requests.Any())
                return BadRequest(new ErrorResponse("No books provided"));

            try
            {
                var result = await _bookService.CreateBooksAsync(requests);

                if (result.DuplicateBooks.Any())
                {
                    return StatusCode(
                        StatusCodes.Status207MultiStatus,
                        new
                        {
                            message = "Some books were created, but some already existed.",
                            result,
                        }
                    );
                }

                return CreatedAtAction(nameof(GetBookTitles), result);
            }
            catch (DuplicateTitleException ex) when (ex.DuplicateBooks.Any())
            {
                return Conflict(new { message = ex.Message, duplicateBooks = ex.DuplicateBooks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating books in bulk");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("An error occurred while processing your request")
                );
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
        {
            try
            {
                await _bookService.UpdateBookAsync(request, id);
            }
            catch (BookNotFoundException)
            {
                return NotFound();
            }
            catch (NoChangesException)
            {
                return BadRequest(new ErrorResponse("No changes were made to the book"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book with ID {BookId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("An error occurred while processing your request")
                );
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                await _bookService.DeleteBookAsync(id);
                return NoContent();
            }
            catch (BookNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book with ID {BookId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("An error occurred while processing your request")
                );
            }
        }

        [HttpDelete("bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBooks([FromBody] IEnumerable<int> ids)
        {
            if (ids?.Any() != true)
                return BadRequest(new ErrorResponse("No book IDs provided"));

            try
            {
                var result = await _bookService.DeleteBooksAsync(ids);
                return result.NotFoundIds.Any()
                    ? Ok(
                        new
                        {
                            message = "Some books were deleted, but some were not found.",
                            notFoundIds = result.NotFoundIds,
                        }
                    )
                    : NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting books in bulk");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("An error occurred while processing your request")
                );
            }
        }
    }
}
