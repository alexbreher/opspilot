namespace OpsPilot.Api.Domain.Entities;

public class Postmortem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IncidentId { get; set; }

    public string Summary { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string LessonsLearned { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}