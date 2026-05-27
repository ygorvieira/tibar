using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tibar.API.Controllers;
using Tibar.Application.Commands.Auth;
using Tibar.Application.Common;
using Tibar.Application.DTOs.Auth;
using Xunit;

namespace Tibar.UnitTests.API;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        var token = new TokenResponse("jwt", "a@b.com", "Alice", DateTime.UtcNow.AddHours(8));

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<RegisterUserCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(token));

        var request = new RegisterRequest("Alice", "a@b.com", "123456");
        var result = await _controller.Register(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(token, ok.Value);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        _mediatorMock.Setup(m => m.Send(
                It.IsAny<RegisterUserCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TokenResponse>("Email already registered."));

        var request = new RegisterRequest("Alice", "existing@b.com", "123456");
        var result = await _controller.Register(request);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(bad.Value);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var token = new TokenResponse("jwt", "a@b.com", "Alice", DateTime.UtcNow.AddHours(8));

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<LoginUserCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(token));

        var request = new LoginRequest("a@b.com", "123456");
        var result = await _controller.Login(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(token, ok.Value);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        _mediatorMock.Setup(m => m.Send(
                It.IsAny<LoginUserCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TokenResponse>("Invalid email or password."));

        var request = new LoginRequest("wrong@b.com", "wrong");
        var result = await _controller.Login(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
