namespace Lwt.Test.Services
{
    using System;
    using System.Threading.Tasks;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Services;
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
        private readonly Mock<ITokenProvider> tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceTest"/> class.
        /// a.
        /// </summary>
        public UserServiceTest()
        {
            var userStore = new Mock<IUserStore<User>>();

            this.userManager =
                new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

            this.tokenProvider = new Mock<ITokenProvider>();
            this.userService = new UserService(this.userManager.Object, this.tokenProvider.Object);
        }

        /// <summary>
        /// test login throw bad request if user does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsync_ShouldThrowBadRequest_IfUserNotExist()
        {
            // arrange
            var userName = "userName";
            var password = "password";
            this.userManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync((User)null);


            // act

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.userService.LoginAsync(userName, password));
        }

        /// <summary>
        /// login should throw bad request if the password is wrong.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsync_ShouldThrowBadRequest_IfWrongPassword()
        {
            // arrange
            var userName = "userName";
            var password = "password";
            User user = new User();
            this.userManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
            this.userManager.Setup(m => m.CheckPasswordAsync(user, password)).ReturnsAsync(false);

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.userService.LoginAsync(userName, password));
        }

        /// <summary>
        /// login should return authentication token if success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsync_ShouldReturnToken_IfSuccess()
        {
            // arrange
            var userName = "user";
            var password = "pass";
            User user = new User();
            var token = Guid.NewGuid().ToString();
            this.userManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
            this.userManager.Setup(m => m.CheckPasswordAsync(user, password)).ReturnsAsync(true);
            this.tokenProvider.Setup(p => p.GenerateUserToken(user)).Returns(token);


            // act
            string actual = await this.userService.LoginAsync(userName, password);

            // assert
            Assert.Equal(token, actual);
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
                .ReturnsAsync(IdentityResult.Failed(new IdentityError {Description = "Hello"}));

            // act

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.userService.SignUpAsync(userName, password));
        }
    }
}