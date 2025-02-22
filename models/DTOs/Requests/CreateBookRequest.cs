using System.ComponentModel.DataAnnotations;

namespace BookManager.Models.DTOs.Requests
{
    public class CreateBookRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public required string Title { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public required string Author { get; set; }

        [Required]
        [Range(1300, 2025)]
        public int PublicationYear { get; set; }
    }
}
