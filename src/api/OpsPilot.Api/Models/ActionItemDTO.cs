namespace OpsPilot.Api.Models;

public class ActionItemDto
{
    public Guid Id { get; set; }
    public Guid PostmortemId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}