namespace Lwt.Test.Controllers;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Lwt.Controllers;
using Lwt.Interfaces;
using Lwt.Interfaces.Services;
using Lwt.Models;
using Lwt.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

/// <inheritdoc />
/// <summary>
/// a.
/// </summary>
public class TextControllerTest : IDisposable
{
    private readonly TextController textController;

    private readonly Mock<ITextService> textService;

    private readonly Mock<IAuthenticationHelper> authenticationHelper;

    private readonly Mock<IMapper<TextCreateModel, int, Text>> textCreateMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextControllerTest"/> class.
    /// a.
    /// </summary>
    public TextControllerTest()
    {
        this.textService = new Mock<ITextService>();
        this.textCreateMapper = new Mock<IMapper<TextCreateModel, int, Text>>();
        this.authenticationHelper = new Mock<IAuthenticationHelper>();

        this.textController =
            new TextController(
                this.textService.Object,
                this.authenticationHelper.Object,
                this.textCreateMapper.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext(), },
            };
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CreateAsyncShouldCallService()
    {
        // arrange
        this.textService.Reset();
        var model = new TextCreateModel();
        var text = new Text();
        var userId = 1;
        this.textCreateMapper.Setup(m => m.Map(model, userId)).Returns(text);
        this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.textController.User)).Returns(userId);
        this.textService.Setup(s => s.CreateAsync(text)).Returns(Task.FromResult(1));

        // act
        await this.textController.CreateAsync(model);

        // assert
        this.textService.Verify(s => s.CreateAsync(text), Times.Once);
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CreateAsyncShouldReturnOkIfSuccess()
    {
        // arrange
        this.textService.Setup(m => m.CreateAsync(It.IsAny<Text>())).Returns(Task.FromResult(1));

        // act
        IActionResult actual = await this.textController.CreateAsync(new TextCreateModel());

        // assert
        Assert.IsType<OkObjectResult>(actual);
    }

    /// <summary>
    /// get all should return ok if the action success.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetAllAsyncShouldReturnOkIfSuccess()
    {
        // arrange
        var userId = 1;
        var filter = new TextFilter();
        var paginationQuery = new PaginationQuery();
        TextViewModel[] texts = Array.Empty<TextViewModel>();
        this.textService.Setup(s => s.GetByUserAsync(userId, filter, paginationQuery)).ReturnsAsync(texts);
        this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.textController.User)).Returns(userId);

        // act
        IActionResult actual = await this.textController.GetAllAsync(filter, paginationQuery);

        // assert
        Assert.IsType<OkObjectResult>(actual);
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DeleteAsyncShouldCallService()
    {
        // arrange
        var id = 1;
        var userId = 1;
        this.authenticationHelper.Setup(h => h.GetLoggedInUser(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        // act
        await this.textController.DeleteAsync(id);

        // assert
        this.textService.Verify(s => s.DeleteAsync(id, userId), Times.Once);
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DeleteAsyncShouldReturnOk()
    {
        // arrange
        var id = 1;
        var userId = 1;
        this.authenticationHelper.Setup(h => h.GetLoggedInUser(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        // act
        IActionResult actual = await this.textController.DeleteAsync(id);

        // assert
        Assert.IsType<OkResult>(actual);
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EditAsyncShouldReturnOk()
    {
        // arrange
        var id = 1;
        var editModel = new TextEditModel();

        // act
        IActionResult actual = await this.textController.EditAsync(id, editModel);

        // assert
        Assert.IsType<OkResult>(actual);
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EditAsyncShouldCallService()
    {
        // arrange
        var textId = 1;
        var userId = 1;
        var editModel = new TextEditModel();
        this.authenticationHelper.Setup(h => h.GetLoggedInUser(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        // act
        await this.textController.EditAsync(textId, editModel);

        // assert
        this.textService.Verify(s => s.EditAsync(textId, userId, editModel), Times.Once);
    }

    /// <summary>
    /// count api should work as expected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetAsyncShouldReturnCorrectCount()
    {
        var userId = 1;
        var filter = new TextFilter();
        var pagingQuery = new PaginationQuery();
        int count = new Random().Next(0, 10000);
        this.textService.Setup(t => t.CountAsync(userId, filter)).ReturnsAsync(count);
        this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.textController.User)).Returns(userId);

        IActionResult actual = await this.textController.GetAllAsync(filter, pagingQuery);

        var result = Assert.IsType<OkObjectResult>(actual);
        var obj = Assert.IsType<TextList>(result.Value);
        Assert.Equal(count, obj.Total);
    }

    /// <summary>
    ///  read async should return right text read model.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadAsyncShouldReturnRightText()
    {
        var userId = 1;
        var id = 1;
        var textReadModel = new TextReadModel();
        this.textService
            .Setup(t => t.ReadAsync(id, userId))
            .ReturnsAsync(textReadModel);
        this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.textController.User)).Returns(userId);

        IActionResult actual = await this.textController.ReadAsync(id);

        var result = Assert.IsType<OkObjectResult>(actual);
        var obj = Assert.IsType<TextReadModel>(result.Value);
        Assert.Equal(textReadModel, obj);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <param name="disposing">disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
    }
}