namespace OpsPilot.Api.Domain.Entities;

public class ActionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PostmortemId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open | InProgress | Done
    public DateTime DueDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}