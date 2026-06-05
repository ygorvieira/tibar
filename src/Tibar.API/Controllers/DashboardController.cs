using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tibar.Application.Queries.Dashboard;

namespace Tibar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? type = null)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(
            new GetBalanceByPeriodQuery(userId, startDate, endDate, categoryId, type));

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyBalances(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? type = null)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(
            new GetMonthlyBalancesQuery(userId, startDate, endDate, categoryId, type));

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
