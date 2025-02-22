using System.ComponentModel.DataAnnotations;

namespace BookManager.Models.DTOs.Requests
{
    public class UpdateBookRequest
    {
        [StringLength(200, MinimumLength = 2)]
        public required string Title { get; set; }

        [StringLength(200, MinimumLength = 2)]
        public required string Author { get; set; }

        [Range(1300, 2025)]
        public int PublicationYear { get; set; }
    }
}
