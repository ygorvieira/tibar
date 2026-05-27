using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tibar.API.Controllers;
using Tibar.Application.Common;
using Tibar.Application.DTOs;
using Tibar.Application.Queries.Dashboard;
using Xunit;

namespace Tibar.UnitTests.API;

public class DashboardControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly DashboardController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public DashboardControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new DashboardController(_mediatorMock.Object);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };
    }

    [Fact]
    public async Task GetBalance_ReturnsOk()
    {
        var balance = new BalanceDto(5000, 2000, 3000, "BRL",
            new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<GetBalanceByPeriodQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(balance));

        var result = await _controller.GetBalance(
            new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(balance, ok.Value);
    }
}
