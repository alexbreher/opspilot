namespace OpsPilot.Api.Services;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public string QueueIncidentCreated { get; set; } = "opspilot.incident.created";
    public string QueueIncidentStatusChanged { get; set; } = "opspilot.incident.status_changed";
}