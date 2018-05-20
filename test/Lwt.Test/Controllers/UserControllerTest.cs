using System.Threading.Tasks;
using Lwt.Controllers;
using Lwt.DbContexts;
using Lwt.Interfaces.Services;
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
        private readonly UserController _userController;
        public UserControllerTest()
        {
            _userService = new Mock<IUserService>();
            _userController = new UserController(_userService.Object);
        }

        #region Constructor
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
        #endregion

        #region SignUp

        [Fact]
        public async Task SignUp_ShouldReturnBadRequest_IfViewModelNotValid()
        {
            // arrange
            _userController.ModelState.AddModelError("error", "message");

            // act
            IActionResult result = await _userController.SignUpAsync(null);

            // assert
            Assert.IsType<BadRequestResult>(result);
        }


        [Fact]
        public async Task SignUp_ShouldReturnOk_IfSignUpSuccess()
        {
            // arrange
            var userName = "hai";
            var passWord = "123";
            var signUpViewModel = new SignUpViewModel()
            {
                UserName = userName,
                Password = passWord
            };
            _userService.Setup(s => s.SignUpAsync(userName, passWord)).ReturnsAsync(true);


            // act
            IActionResult result = await _userController.SignUpAsync(signUpViewModel);

            // assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SignUp_ShouldReturnBadRequest_IfSignUpFail()
        {
            // arrange
            var signUpViewModel = new SignUpViewModel();


            _userService.Setup(s => s.SignUpAsync("userName", "passWord")).ReturnsAsync(false);

            // act
            IActionResult result = await _userController.SignUpAsync(signUpViewModel);

            // assert
            Assert.IsType<BadRequestResult>(result);
        }

        #endregion

        #region Login
        [Fact]
        public async Task Login_ShouldReturnBadRequest_IfSignUpFail()
        {
            // arrange
            var viewModel = new LoginViewModel();
            _userController.ModelState.AddModelError("error", "message");

            // act
            IActionResult actual = await _userController.LoginAsync(viewModel);

            //assert
            Assert.IsType<BadRequestResult>(actual);
        }


        [Fact]
        public async Task Login_ShouldReturnOk_IfLoginSuccessful()
        {
            // arrange
            var viewModel = new LoginViewModel();
            _userService.Reset();
            _userService.Setup(s => s.LoginAsync(viewModel.UserName, viewModel.Password)).ReturnsAsync(true);

            // act
            IActionResult actual = await _userController.LoginAsync(viewModel);

            //assert
            Assert.IsType<OkResult>(actual);

        }


        [Fact]
        public async Task Login_ShouldBadRequest_IfLoginFail()
        {
            // arrange
            var viewModel = new LoginViewModel();
            _userService.Reset();
            _userService.Setup(s => s.LoginAsync(viewModel.UserName, viewModel.Password)).ReturnsAsync(false);

            // act
            IActionResult actual = await _userController.LoginAsync(viewModel);

            //assert
            Assert.IsType<BadRequestResult>(actual);

        }
        #endregion

        #region Logout

        [Fact]
        public async Task Logout_ShouldReturnOk()
        {
            // arrange


            // act
            IActionResult actual = await _userController.LogoutAsync();

            // assert
            Assert.IsType<OkResult>(actual);
        }

        [Fact]
        public async Task Logout_ShouldCallLogoutServiceOnce()
        {
            // arrange
            _userService.Reset();

            // act
            await _userController.LogoutAsync();

            // assert
            _userService.Verify(s=>s.LogoutAsync(),Times.Once);
        }

        #endregion
    }
}