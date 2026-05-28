using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tibar.Application.Commands.Auth.Login;
using Tibar.Application.Commands.Auth.Register;
using Tibar.Application.DTOs.Auth;

namespace Tibar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[EnableRateLimiting("Auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

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
