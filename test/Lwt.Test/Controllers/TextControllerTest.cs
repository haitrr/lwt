using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Lwt.Controllers;
using Lwt.Interfaces;
using Lwt.Interfaces.Services;
using Lwt.Models;
using Lwt.ViewModels;
using LWT.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Lwt.Test.Controllers
{
    public class TextControllerTest
    {
        private readonly TextController _textController;
        private readonly Mock<ITextService> _textService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IAuthenticationHelper> _authenticationHelper;

        public TextControllerTest()
        {
            _textService = new Mock<ITextService>();
            _mapper = new Mock<IMapper>();
            _authenticationHelper = new Mock<IAuthenticationHelper>();

            _textController = new TextController(_textService.Object, _mapper.Object, _authenticationHelper.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task CreateAsync_ShouldCallService()
        {
            // arrange
            _textService.Reset();
            var model = new TextCreateModel();
            var text = new Text();
            Guid userId = Guid.NewGuid();
            _mapper.Setup(m => m.Map<Text>(model)).Returns(text);
            _authenticationHelper.Setup(h => h.GetLoggedInUser(_textController.User)).Returns(userId);
            _textService.Setup(s => s.CreateAsync(userId, text)).Returns(Task.CompletedTask);

            // act
            await _textController.CreateAsync(model);

            // assert
            _textService.Verify(s => s.CreateAsync(userId, text), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnOk_IfSuccess()
        {
            // arrange
            _textService.Setup(m => m.CreateAsync(It.IsAny<Guid>(), It.IsAny<Text>())).Returns(Task.CompletedTask);

            // act
            IActionResult actual = await _textController.CreateAsync(new TextCreateModel());

            // assert
            Assert.IsType<OkResult>(actual);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOk_IfSuccess()
        {
            // arrange
            Guid userId = Guid.NewGuid();
            var texts = new List<Text>();
            _textService.Setup(s => s.GetByUserAsync(userId)).ReturnsAsync(texts);
            _authenticationHelper.Setup(h => h.GetLoggedInUser(_textController.User)).Returns(userId);
            _mapper.Setup(m => m.Map<IEnumerable<TextViewModel>>(texts)).Returns(new List<TextViewModel>());

            //act
            IActionResult actual = await _textController.GetAllAsync();

            //assert
            Assert.IsType<OkObjectResult>(actual);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallService()
        {
            // arrange
            Guid id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            _authenticationHelper.Setup(h => h.GetLoggedInUser(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            //act
            await _textController.DeleteAsync(id);

            // assert
            _textService.Verify(s => s.DeleteAsync(id, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnOk()
        {
            // arrange
            Guid id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            _authenticationHelper.Setup(h => h.GetLoggedInUser(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            //act
            IActionResult actual = await _textController.DeleteAsync(id);

            // assert
            Assert.IsType<OkResult>(actual);
        }


        [Fact]
        public async Task EditAsync_ShouldReturnOk()
        {
            // arrange
            Guid id = Guid.NewGuid();
            var editModel = new TextEditModel();

            //act
            IActionResult actual = await _textController.EditAsync(id, editModel);

            // assert
            Assert.IsType<OkResult>(actual);
        }

        [Fact]
        public async Task EditAsync_ShouldCallService()
        {
            // arrange
            Guid textId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var editModel = new TextEditModel();
            _authenticationHelper.Setup(h => h.GetLoggedInUser(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            //act
            await _textController.EditAsync(textId, editModel);

            // assert
            _textService.Verify(s => s.EditAsync(textId, userId, editModel), Times.Once);
        }
    }
}