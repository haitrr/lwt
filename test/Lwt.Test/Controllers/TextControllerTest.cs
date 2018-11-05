namespace Lwt.Test.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using AutoMapper;

    using Lwt.Controllers;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;

    using LWT.Models;

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

        private readonly Mock<IMapper> mapper;

        private readonly Mock<IAuthenticationHelper> authenticationHelper;

        private readonly Mock<IMapper<TextCreateModel, Guid, Text>> textCreateMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextControllerTest"/> class.
        /// a.
        /// </summary>
        public TextControllerTest()
        {
            this.textService = new Mock<ITextService>();
            this.mapper = new Mock<IMapper>();
            this.textCreateMapper = new Mock<IMapper<TextCreateModel, Guid, Text>>();
            this.authenticationHelper = new Mock<IAuthenticationHelper>();

            this.textController = new TextController(
                this.textService.Object,
                this.mapper.Object,
                this.authenticationHelper.Object,
                this.textCreateMapper.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_ShouldCallService()
        {
            // arrange
            this.textService.Reset();
            var model = new TextCreateModel();
            var text = new Text();
            Guid userId = Guid.NewGuid();
            this.textCreateMapper.Setup(m => m.Map(model, userId)).Returns(text);
            this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.textController.User)).Returns(userId);
            this.textService.Setup(s => s.CreateAsync(text)).Returns(Task.CompletedTask);

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
        public async Task CreateAsync_ShouldReturnOk_IfSuccess()
        {
            // arrange
            this.textService.Setup(m => m.CreateAsync(It.IsAny<Text>())).Returns(Task.CompletedTask);

            // act
            IActionResult actual = await this.textController.CreateAsync(new TextCreateModel());

            // assert
            Assert.IsType<OkResult>(actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAllAsync_ShouldReturnOk_IfSuccess()
        {
            // arrange
            Guid userId = Guid.NewGuid();
            var texts = new List<Text>();
            this.textService.Setup(s => s.GetByUserAsync(userId)).ReturnsAsync(texts);
            this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.textController.User)).Returns(userId);
            this.mapper.Setup(m => m.Map<IEnumerable<TextViewModel>>(texts)).Returns(new List<TextViewModel>());

            // act
            IActionResult actual = await this.textController.GetAllAsync();

            // assert
            Assert.IsType<OkObjectResult>(actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_ShouldCallService()
        {
            // arrange
            Guid id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
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
        public async Task DeleteAsync_ShouldReturnOk()
        {
            // arrange
            Guid id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
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
        public async Task EditAsync_ShouldReturnOk()
        {
            // arrange
            Guid id = Guid.NewGuid();
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
        public async Task EditAsync_ShouldCallService()
        {
            // arrange
            Guid textId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var editModel = new TextEditModel();
            this.authenticationHelper.Setup(h => h.GetLoggedInUser(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            // act
            await this.textController.EditAsync(textId, editModel);

            // assert
            this.textService.Verify(s => s.EditAsync(textId, userId, editModel), Times.Once);
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
}