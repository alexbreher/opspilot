using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/services/{serviceId:guid}/runbooks")]
public class ServiceRunbooksController : ControllerBase
{
    private readonly RunbookService _runbookService;

    public ServiceRunbooksController(RunbookService runbookService)
    {
        _runbookService = runbookService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RunbookDto>), StatusCodes.Status200OK)]
    public IActionResult GetByService(Guid serviceId)
    {
        var result = _runbookService.GetByServiceId(serviceId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RunbookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create(Guid serviceId, [FromBody] CreateRunbookRequest request)
    {
        if (serviceId == Guid.Empty) return BadRequest("ServiceId is required.");
        if (request == null) return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Title is required.");
        if (string.IsNullOrWhiteSpace(request.ContentMarkdown)) return BadRequest("ContentMarkdown is required.");
        if (string.IsNullOrWhiteSpace(request.CreatedBy)) return BadRequest("CreatedBy is required.");

        try
        {
            var created = _runbookService.Create(serviceId, request);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}