using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tibar.API.Controllers;
using Tibar.Application.Commands.Categories.Create;
using Tibar.Application.Commands.Categories.Delete;
using Tibar.Application.Commands.Categories.Update;
using Tibar.Application.Common;
using Tibar.Application.DTOs;
using Tibar.Application.Queries.Categories;
using Xunit;

namespace Tibar.UnitTests.API;

public class CategoriesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CategoriesController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public CategoriesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CategoriesController(_mediatorMock.Object);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        var categories = new List<CategoryDto>
        {
            new(Guid.NewGuid(), "Food", "Expense", DateTime.UtcNow)
        };

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<GetCategoriesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<CategoryDto>>(categories));

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(categories, ok.Value);
    }

    [Fact]
    public async Task Create_ValidCommand_ReturnsOk()
    {
        var dto = new CategoryDto(Guid.NewGuid(), "Food", "Expense", DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<CreateCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(dto));

        var command = new CreateCategoryCommand("Food", "Expense", Guid.NewGuid());
        var result = await _controller.Create(command);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Create_SetsUserIdFromClaims()
    {
        _mediatorMock.Setup(m => m.Send(
                It.Is<CreateCategoryCommand>(c => c.UserId == _userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CategoryDto>("error"));

        var command = new CreateCategoryCommand("Food", "Expense", Guid.NewGuid());
        await _controller.Create(command);

        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateCategoryCommand>(c => c.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ValidCommand_ReturnsOk()
    {
        var dto = new CategoryDto(Guid.NewGuid(), "Updated", "Income", DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(
                It.IsAny<UpdateCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(dto));

        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Updated", "Income", _userId);
        var result = await _controller.Update(Guid.NewGuid(), command);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(
                It.IsAny<DeleteCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }
}
