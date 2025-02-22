using BookManager.Models.DTOs.Responses;

namespace BookManager.Exceptions
{
    public class DuplicateTitleException : Exception
    {
        public IEnumerable<DuplicateBookInfo> DuplicateBooks { get; }

        public DuplicateTitleException(string message)
            : base(message)
        {
            DuplicateBooks = new List<DuplicateBookInfo>();
        }

        public DuplicateTitleException(
            string message,
            IEnumerable<DuplicateBookInfo> duplicateBooks
        )
            : base(message)
        {
            DuplicateBooks = duplicateBooks;
        }
    }
}
