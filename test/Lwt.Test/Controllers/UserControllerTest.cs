using System.Threading.Tasks;
using Lwt.Controllers;
using Lwt.Interfaces.Services;
using Lwt.ViewModels.User;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;

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

        [Fact]
        public async Task SignUp_ShouldReturnOk_IfSignUpSuccess()
        {
            // arrange
            var userName = "hai";
            var passWord = "123";

            var signUpViewModel = new SignUpViewModel
            {
                UserName = userName,
                Password = passWord
            };

            // act
            IActionResult result = await _userController.SignUpAsync(signUpViewModel);

            // assert
            Assert.IsType<OkResult>(result);
        }

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
            _userService.Verify(s => s.LogoutAsync(), Times.Once);
        }
    }
}