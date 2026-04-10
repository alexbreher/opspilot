namespace OpsPilot.Api.Models;

public class CreateRunbookRequest
{
    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}