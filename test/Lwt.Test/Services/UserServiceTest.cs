namespace Lwt.Test.Services
{
    using System.Threading.Tasks;
    using Lwt.Exceptions;
    using Lwt.Models;
    using Lwt.Services;
    using Lwt.ViewModels.User;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserServiceTest.
    /// </summary>
    public class UserServiceTest
    {
        private readonly UserService userService;
        private readonly Mock<UserManager<User>> userManager;
        private readonly Mock<SignInManager<User>> signInManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceTest"/> class.
        /// a.
        /// </summary>
        public UserServiceTest()
        {
            var userStore = new Mock<IUserStore<User>>();

            this.userManager =
                new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

            this.signInManager = new Mock<SignInManager<User>>(
                this.userManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null,
                null,
                null);
            this.userService = new UserService(this.userManager.Object, this.signInManager.Object);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_IfLoginFail()
        {
            // arrange
            var model = new LoginViewModel();

            this.signInManager.Setup(m => m.PasswordSignInAsync(model.UserName, model.Password, false, false))
                .ReturnsAsync(SignInResult.Failed);

            // act
            bool actual = await this.userService.LoginAsync(model.UserName, model.Password);

            // assert
            Assert.False(actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsync_ShouldReturnTrue_IfSuccess()
        {
            // arrange
            var model = new LoginViewModel();

            this.signInManager.Setup(m => m.PasswordSignInAsync(model.UserName, model.Password, false, false))
                .ReturnsAsync(SignInResult.Success);

            // act
            bool actual = await this.userService.LoginAsync(model.UserName, model.Password);

            // assert
            Assert.True(actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LogoutAsync_ShouldCallLogoutService()
        {
            // arrange
            this.signInManager.Reset();

            // act
            await this.userService.LogoutAsync();

            // assert
            this.signInManager.Verify(m => m.SignOutAsync(), Times.Once);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUp_ShouldCallUserManageCreate_Once()
        {
            // arrange
            this.userManager.Reset();
            this.userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), "pass")).ReturnsAsync(IdentityResult.Success);

            // act
            await this.userService.SignUpAsync("user", "pass");

            // assert
            this.userManager.Verify(m => m.CreateAsync(It.IsAny<User>(), "pass"), Times.Once);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUp_ShouldReturnNotThrow_IfCreateUserSuccess()
        {
            // arrange
            this.userManager.Reset();
            var userName = "user";
            var password = "pass";

            this.userManager.Setup(m => m.CreateAsync(It.Is<User>(u => u.UserName == userName), password))
                .ReturnsAsync(IdentityResult.Success);

            // act
            await this.userService.SignUpAsync(userName, password);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUp_ShouldThrowException_IfFailToCreateUser()
        {
            // arrange
            this.userManager.Reset();
            var userName = "user";
            var password = "pass";

            this.userManager.Setup(m => m.CreateAsync(It.Is<User>(u => u.UserName == userName), password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Hello" }));

            // act

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.userService.SignUpAsync(userName, password));
        }
    }
}