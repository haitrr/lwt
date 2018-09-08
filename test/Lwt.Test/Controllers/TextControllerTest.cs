using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Lwt.Controllers;
using Lwt.DbContexts;
using Lwt.Interfaces;
using Lwt.Interfaces.Services;
using Lwt.ViewModels;
using LWT.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace Lwt.Test.Controllers
{
    public class TextControllerTest : TestClass
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
        public void Constructor_ShouldWork()
        {
            // arrange
            ServiceProvider efServiceProvider =
                new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

            var services = new ServiceCollection();

            services.AddDbContext<LwtDbContext>(b =>
                b.UseInMemoryDatabase("Lwt").UseInternalServiceProvider(efServiceProvider));
            var configuration = new Mock<IConfiguration>();
            var startup = new Startup();
            startup.ConfigureServices(services);
            services.AddTransient<TextController>();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // act
            var instance = serviceProvider.GetService<TextController>();

            // assert
            Assert.NotNull(instance);
        }


        [Fact]
        public async Task CreateAsync_ShouldCallService()
        {
            // arrange
            _textService.Reset();
            TextCreateModel model = new TextCreateModel();
            Text text = new Text();
            Guid userId = Guid.NewGuid();
            _mapper.Setup(m => m.Map<Text>(model)).Returns(text);
            _authenticationHelper.Setup(h => h.GetLoggedInUser(_textController.User.Identity)).Returns(userId);

            // act
            await _textController.CreateAsync(model);

            // assert
            _textService.Verify(s => s.CreateAsync(userId, text), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnOk_IfSuccess()
        {
            // arrange
            _textService.Setup(m => m.CreateAsync(It.IsAny<Guid>(), It.IsAny<Text>())).ReturnsAsync(true);

            // act
            var actual = await _textController.CreateAsync(new TextCreateModel());

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
            var textViewModels = new List<TextViewModel>();
            _authenticationHelper.Setup(h => h.GetLoggedInUser(_textController.User.Identity)).Returns(userId);
            _mapper.Setup(m => m.Map<IEnumerable<TextViewModel>>(texts)).Returns(textViewModels);

            //act
            IActionResult actual = await _textController.GetAllAsync();

            //assert
            Assert.IsType<OkObjectResult>(actual);
        }
    }
}