namespace BookManager.Models.DTOs.Responses
{
    public class BookTitleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public double PopularityScore { get; set; }
    }
}
