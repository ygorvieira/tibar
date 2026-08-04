using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tibar.Application.Commands.Accounts.Create;
using Tibar.Application.Commands.Accounts.Delete;
using Tibar.Application.Commands.Accounts.Update;
using Tibar.Application.Queries.Accounts;

namespace Tibar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var result = await _mediator.Send(
            new GetAccountsQuery(userId));

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand command)
    {
        var userId = GetUserId();
        command = command with { UserId = userId };

        var result = await _mediator.Send(command);

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountCommand command)
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
            new DeleteAccountCommand(id, userId));

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
