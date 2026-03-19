namespace OpsPilot.Api.Models;

public class UpdateIncidentStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}