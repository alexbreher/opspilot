namespace OpsPilot.Api.Models;

public class RunbookDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}