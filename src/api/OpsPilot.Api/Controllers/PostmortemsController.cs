using Microsoft.AspNetCore.Mvc;
using OpsPilot.Api.Models;
using OpsPilot.Api.Services;

namespace OpsPilot.Api.Controllers;

[ApiController]
[Route("api/postmortems/{postmortemId:guid}/action-items")]
public class PostmortemsController : ControllerBase
{
    private readonly PostmortemService _postmortemService;

    public PostmortemsController(PostmortemService postmortemService)
    {
        _postmortemService = postmortemService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ActionItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AddActionItem(Guid postmortemId, [FromBody] CreateActionItemRequest request)
    {
        if (postmortemId == Guid.Empty) return BadRequest("PostmortemId is required.");
        if (request == null) return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Title is required.");
        if (string.IsNullOrWhiteSpace(request.Owner)) return BadRequest("Owner is required.");
        if (request.DueDateUtc == default) return BadRequest("DueDateUtc is required.");

        try
        {
            var created = _postmortemService.AddActionItem(postmortemId, request);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}