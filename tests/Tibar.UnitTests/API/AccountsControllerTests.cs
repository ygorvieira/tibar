using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tibar.API.Controllers;
using Tibar.Application.Commands.Accounts.Create;
using Tibar.Application.Commands.Accounts.Delete;
using Tibar.Application.Commands.Accounts.Update;
using Tibar.Application.Common;
using Tibar.Application.DTOs;
using Tibar.Application.Queries.Accounts;
using Xunit;

namespace Tibar.UnitTests.API;

public class AccountsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AccountsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public AccountsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AccountsController(_mediatorMock.Object);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        var accounts = new List<AccountDto>
        {
            new(Guid.NewGuid(), "Bradesco", "Checking", DateTime.UtcNow)
        };

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<GetAccountsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<AccountDto>>(accounts));

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(accounts, ok.Value);
    }

    [Fact]
    public async Task Create_ValidCommand_ReturnsOk()
    {
        var dto = new AccountDto(Guid.NewGuid(), "Nu", "Investment", DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<CreateAccountCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(dto));

        var command = new CreateAccountCommand("Nu", "investment", Guid.NewGuid());
        var result = await _controller.Create(command);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Create_SetsUserIdFromClaims()
    {
        _mediatorMock.Setup(m => m.Send(
                It.Is<CreateAccountCommand>(c => c.UserId == _userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AccountDto>("error"));

        var command = new CreateAccountCommand("Nu", "investment", Guid.NewGuid());
        await _controller.Create(command);

        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateAccountCommand>(c => c.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ValidCommand_ReturnsOk()
    {
        var dto = new AccountDto(Guid.NewGuid(), "Updated", "Checking", DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<UpdateAccountCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(dto));

        var command = new UpdateAccountCommand(Guid.NewGuid(), "Updated", "checking", _userId);
        var result = await _controller.Update(Guid.NewGuid(), command);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(
                It.IsAny<DeleteAccountCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WithTransactions_ReturnsBadRequest()
    {
        _mediatorMock.Setup(m => m.Send(
                It.IsAny<DeleteAccountCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>("Conta possui transações vinculadas e não pode ser excluída."));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
