namespace BookManager.Models.DTOs.Responses
{
    public class ErrorResponse
    {
        public string Message { get; set; } = null!;

        public ErrorResponse(string message) => Message = message;
    }
}
