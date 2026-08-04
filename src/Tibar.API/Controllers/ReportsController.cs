using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tibar.Application.Queries.Reports;

namespace Tibar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("expenses-by-category")]
    public async Task<IActionResult> GetExpensesByCategory(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? accountId = null)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(
            new GetExpenseReportQuery(userId, startDate, endDate, accountId));

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
