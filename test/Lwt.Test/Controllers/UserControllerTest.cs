namespace Lwt.Test.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Lwt.Controllers;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;
    using Lwt.ViewModels.User;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <inheritdoc />
    /// <summary>
    /// a.
    /// </summary>
    public class UserControllerTest : System.IDisposable
    {
        private readonly Mock<IUserService> userService;
        private readonly Mock<IAuthenticationHelper> authenticationHelper;

        private readonly UserController userController;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserControllerTest"/> class.
        /// </summary>
        public UserControllerTest()
        {
            this.userService = new Mock<IUserService>();
            this.authenticationHelper = new Mock<IAuthenticationHelper>();
            this.userController = new UserController(this.userService.Object, this.authenticationHelper.Object);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUpShouldReturnOkIfSignUpSuccess()
        {
            // arrange
            var userName = "hai";
            var passWord = "123";

            var signUpViewModel = new SignUpViewModel { UserName = userName, Password = passWord, };

            // act
            IActionResult result = await this.userController.SignUpAsync(signUpViewModel);

            // assert
            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginShouldReturnBadRequestIfSignUpFail()
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
        /// test if login return token if success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginShouldReturnOkIfLoginSuccessful()
        {
            // arrange
            var viewModel = new LoginViewModel();
            this.userService.Reset();
            string token = 1.ToString();
            this.userService.Setup(s => s.LoginAsync(viewModel.UserName, viewModel.Password)).ReturnsAsync(token);

            // act
            IActionResult actual = await this.userController.LoginAsync(viewModel);

            // assert
            var result = Assert.IsType<OkObjectResult>(actual);
            var resultValue = Assert.IsType<LoginResult>(result.Value);
            Assert.Equal(token, resultValue.Token);
        }

        /// <inheritdoc />
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
                this.userController.Dispose();
            }
        }
    }
}