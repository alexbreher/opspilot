using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunbooksController : ControllerBase
{
    private readonly RunbookService _runbookService;

    public RunbooksController(RunbookService runbookService)
    {
        _runbookService = runbookService;
    }

    [HttpGet("{runbookId:guid}")]
    [ProducesResponseType(typeof(RunbookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid runbookId)
    {
        var rb = _runbookService.GetById(runbookId);
        if (rb == null) return NotFound("Runbook not found.");

        return Ok(rb);
    }
}