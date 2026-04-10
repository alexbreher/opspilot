namespace OpsPilot.Api.Domain.Entities;

public class Runbook
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}