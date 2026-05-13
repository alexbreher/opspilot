using Microsoft.AspNetCore.Mvc;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageBusStatusController : ControllerBase
{
    private readonly IConfiguration _config;

    public MessageBusStatusController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            transport = _config["MessageBus:Transport"],
            rabbitHost = _config["MessageBus:RabbitMq:Host"]
        });
    }
}