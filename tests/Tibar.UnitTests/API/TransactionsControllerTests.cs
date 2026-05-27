using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tibar.API.Controllers;
using Tibar.Application.Commands.Transactions;
using Tibar.Application.Common;
using Tibar.Application.DTOs;
using Tibar.Application.Queries.Transactions;
using Xunit;

namespace Tibar.UnitTests.API;

public class TransactionsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly TransactionsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public TransactionsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TransactionsController(_mediatorMock.Object);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };
    }

    [Fact]
    public async Task GetTransactions_ReturnsOkWithList()
    {
        var transactions = new List<TransactionDto>
        {
            new(Guid.NewGuid(), "Test", 100, "BRL", "Expense", Guid.NewGuid(), "Food",
                new DateOnly(2026, 5, 27), DateTime.UtcNow)
        };

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<GetTransactionsByPeriodQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<TransactionDto>>(transactions));

        var result = await _controller.GetTransactions(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(transactions, ok.Value);
    }

    [Fact]
    public async Task Create_ValidCommand_ReturnsOk()
    {
        var dto = new TransactionDto(Guid.NewGuid(), "New", 50, "BRL", "Expense",
            Guid.NewGuid(), "Food", new DateOnly(2026, 5, 27), DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<CreateTransactionCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(dto));

        var command = new CreateTransactionCommand("New", 50, "Expense", Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 5, 27));
        var result = await _controller.Create(command);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Create_WithUserOverride_SetsUserIdFromClaims()
    {
        _mediatorMock.Setup(m => m.Send(
                It.Is<CreateTransactionCommand>(c => c.UserId == _userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TransactionDto>("error"));

        var command = new CreateTransactionCommand("New", 50, "Expense", Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 5, 27));
        await _controller.Create(command);

        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateTransactionCommand>(c => c.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ValidCommand_ReturnsOk()
    {
        var dto = new TransactionDto(Guid.NewGuid(), "Updated", 75, "BRL", "Expense",
            Guid.NewGuid(), "Food", new DateOnly(2026, 5, 27), DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<UpdateTransactionCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(dto));

        var command = new UpdateTransactionCommand(Guid.NewGuid(), "Updated", 75, Guid.NewGuid(), _userId, new DateOnly(2026, 5, 27));
        var result = await _controller.Update(Guid.NewGuid(), command);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(
                It.IsAny<DeleteTransactionCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_Failure_ReturnsBadRequest()
    {
        _mediatorMock.Setup(m => m.Send(
                It.IsAny<DeleteTransactionCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>("Not found."));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
