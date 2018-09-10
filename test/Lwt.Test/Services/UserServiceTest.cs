using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Models;
using Lwt.Services;
using Lwt.ViewModels.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Lwt.Test.Services
{
    public class UserServiceTest
    {
        public UserServiceTest()
        {
            var userStore = new Mock<IUserStore<User>>();

            _userManager =
                new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

            _signInManager = new Mock<SignInManager<User>>(_userManager.Object, new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object, null, null, null);
            _userService = new UserService(_userManager.Object, _signInManager.Object);
        }

        private readonly UserService _userService;
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<SignInManager<User>> _signInManager;

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_IfLoginFail()
        {
            // arrange
            var model = new LoginViewModel();

            _signInManager.Setup(m => m.PasswordSignInAsync(model.UserName, model.Password, false, false))
                .ReturnsAsync(SignInResult.Failed);

            // act
            bool actual = await _userService.LoginAsync(model.UserName, model.Password);

            // assert
            Assert.False(actual);
        }


        [Fact]
        public async Task LoginAsync_ShouldReturnTrue_IfSuccess()
        {
            // arrange
            var model = new LoginViewModel();

            _signInManager.Setup(m => m.PasswordSignInAsync(model.UserName, model.Password, false, false))
                .ReturnsAsync(SignInResult.Success);

            // act
            bool actual = await _userService.LoginAsync(model.UserName, model.Password);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public async Task LogoutAsync_ShouldCallLogoutService()
        {
            // arrange
            _signInManager.Reset();

            //act
            await _userService.LogoutAsync();

            // assert
            _signInManager.Verify(m => m.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task SignUp_ShouldCallUserManageCreate_Once()
        {
            // arrange
            _userManager.Reset();
            _userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), "pass")).ReturnsAsync(IdentityResult.Success);

            // act
            await _userService.SignUpAsync("user", "pass");

            // assert
            _userManager.Verify(m => m.CreateAsync(It.IsAny<User>(), "pass"), Times.Once);
        }

        [Fact]
        public async Task SignUp_ShouldThrowException_IfFailToCreateUser()
        {
            // arrange
            _userManager.Reset();
            var userName = "user";
            var password = "pass";

            _userManager.Setup(m => m.CreateAsync(It.Is<User>(u => u.UserName == userName), password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError {Description = "Hello"}));

            // act


            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => _userService.SignUpAsync(userName, password));
        }

        [Fact]
        public async Task SignUp_ShouldReturnNotThrow_IfCreateUserSuccess()
        {
            // arrange
            _userManager.Reset();
            var userName = "user";
            var password = "pass";

            _userManager.Setup(m => m.CreateAsync(It.Is<User>(u => u.UserName == userName), password))
                .ReturnsAsync(IdentityResult.Success);

            // act
            await _userService.SignUpAsync(userName, password);
        }
    }
}