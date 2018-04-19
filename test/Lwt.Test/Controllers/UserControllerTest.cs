using System;
using System.Threading.Tasks;
using AutoMapper;
using Lwt.Controllers;
using Lwt.DbContexts;
using Lwt.Interfaces.Services;
using Lwt.Models;
using Lwt.ViewModels.User;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lwt.Test.Controllers
{
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _userService;
        private readonly Mock<IMapper> _mapper;
        private readonly UserController _userController;
        public UserControllerTest()
        {
            _userService = new Mock<IUserService>();
            _mapper = new Mock<IMapper>();
            _userController = new UserController(_userService.Object, _mapper.Object);
        }

        [Fact]
        public void Constructor_ShouldWork()
        {
            // arrange
            ServiceProvider efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

            var services = new ServiceCollection();

            services.AddDbContext<LwtDbContext>(b => b.UseInMemoryDatabase("Lwt").UseInternalServiceProvider(efServiceProvider));
            var configuration = new Mock<IConfiguration>();
            var startup = new Startup(configuration.Object);
            startup.ConfigureServices(services);
            services.AddTransient<UserController>();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // act
            var userController = serviceProvider.GetService<UserController>();

            // assert
            Assert.NotNull(userController);
        }

        [Fact]
        public async Task SignUp_ShouldReturnBadRequest_IfViewModelNotValid()
        {
            // arrange
            _userController.ModelState.AddModelError("error", "message");

            // act
            IActionResult result = await _userController.SignUp(null);

            // assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task SignUp_ShouldReturnThrowException_IfCantMap()
        {
            // arrange
            _mapper.Setup(m => m.Map<User>(It.IsAny<SignUpViewModel>())).Returns((User)null);
            var viewModel = new SignUpViewModel();

            // act

            // assert
            await Assert.ThrowsAsync<NotSupportedException>(() => _userController.SignUp(viewModel));
        }

        [Fact]
        public async Task SignUp_ShouldReturnOk_IfSignUpSuccess()
        {
            // arrange
            var signUpViewModel = new SignUpViewModel();
            var user = new User();
            _mapper.Setup(m => m.Map<User>(signUpViewModel)).Returns(user);
            _userService.Setup(s => s.SignUp(user)).ReturnsAsync(true);


            // act
            IActionResult result = await _userController.SignUp(signUpViewModel);

            // assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SignUp_ShouldReturnBadRequest_IfSignUpFail()
        {
            // arrange
            var signUpViewModel = new SignUpViewModel();
            var user = new User();

            _mapper.Setup(m => m.Map<User>(signUpViewModel)).Returns(user);
            _userService.Setup(s => s.SignUp(user)).ReturnsAsync(false);

            // act
            IActionResult result = await _userController.SignUp(signUpViewModel);

            // assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}