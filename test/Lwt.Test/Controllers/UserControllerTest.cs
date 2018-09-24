namespace Lwt.Test.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Lwt.Controllers;
    using Lwt.Interfaces.Services;
    using Lwt.ViewModels.User;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// a.
    /// </summary>
    public class UserControllerTest : System.IDisposable
    {
        private readonly Mock<IUserService> userService;
        private readonly UserController userController;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserControllerTest"/> class.
        /// </summary>
        public UserControllerTest()
        {
            this.userService = new Mock<IUserService>();
            this.userController = new UserController(this.userService.Object);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUp_ShouldReturnOk_IfSignUpSuccess()
        {
            // arrange
            var userName = "hai";
            var passWord = "123";

            var signUpViewModel = new SignUpViewModel
            {
                UserName = userName,
                Password = passWord,
            };

            // act
            IActionResult result = await this.userController.SignUpAsync(signUpViewModel);

            // assert
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Login_ShouldReturnBadRequest_IfSignUpFail()
        {
            // arrange
            var viewModel = new LoginViewModel();
            this.userController.ModelState.AddModelError("error", "message");

            // act
            IActionResult actual = await this.userController.LoginAsync(viewModel);

            // assert
            Assert.IsType<BadRequestResult>(actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Login_ShouldReturnOk_IfLoginSuccessful()
        {
            // arrange
            var viewModel = new LoginViewModel();
            this.userService.Reset();
            this.userService.Setup(s => s.LoginAsync(viewModel.UserName, viewModel.Password)).ReturnsAsync(true);

            // act
            IActionResult actual = await this.userController.LoginAsync(viewModel);

            // assert
            Assert.IsType<OkResult>(actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Login_ShouldBadRequest_IfLoginFail()
        {
            // arrange
            var viewModel = new LoginViewModel();
            this.userService.Reset();
            this.userService.Setup(s => s.LoginAsync(viewModel.UserName, viewModel.Password)).ReturnsAsync(false);

            // act
            IActionResult actual = await this.userController.LoginAsync(viewModel);

            // assert
            Assert.IsType<BadRequestResult>(actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Logout_ShouldReturnOk()
        {
            // arrange

            // act
            IActionResult actual = await this.userController.LogoutAsync();

            // assert
            Assert.IsType<OkResult>(actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Logout_ShouldCallLogoutServiceOnce()
        {
            // arrange
            this.userService.Reset();

            // act
            await this.userController.LogoutAsync();

            // assert
            this.userService.Verify(s => s.LogoutAsync(), Times.Once);
        }

        /// <summary>
        /// a.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="disposing">asd.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.userController?.Dispose();
            }
        }
    }
}