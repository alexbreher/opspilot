namespace OpsPilot.Api.Models;

public class CreateIncidentRequest
{
    public Guid ServiceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}