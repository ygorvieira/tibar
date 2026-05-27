using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tibar.Application.Commands.Auth;
using Tibar.Application.DTOs.Auth;

namespace Tibar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand(request.Name, request.Email, request.Password);
        var result = await _mediator.Send(command);

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        if (!result.IsValid)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
