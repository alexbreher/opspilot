namespace OpsPilot.Api.Models;

public class CreateActionItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;

    // Use ISO string in Swagger; we'll parse to UTC
    public DateTime DueDateUtc { get; set; }
}