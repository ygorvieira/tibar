using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tibar.Application.Commands.Transactions;
using Tibar.Application.Queries.Transactions;

namespace Tibar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(
            new GetTransactionsByPeriodQuery(userId, startDate, endDate));

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionCommand command)
    {
        var userId = GetUserId();
        command = command with { UserId = userId };

        var result = await _mediator.Send(command);

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionCommand command)
    {
        var userId = GetUserId();
        command = command with { Id = id, UserId = userId };

        var result = await _mediator.Send(command);

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(
            new DeleteTransactionCommand(id, userId));

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
