namespace BookManager.Models.DTOs.Responses
{
    public class BookDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int PublicationYear { get; set; }
        public double PopularityScore { get; set; }
        public int BookViews { get; internal set; }
    }
}
