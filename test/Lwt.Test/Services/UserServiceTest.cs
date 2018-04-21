using System;
using System.Threading.Tasks;
using Lwt.DbContexts;
using Lwt.Models;
using Lwt.Services;
using Lwt.ViewModels.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Lwt.Test.Services
{
    public class UserServiceTest
    {
        private readonly UserService _userService;
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<SignInManager<User>> _signInManager;

        public UserServiceTest()
        {
            var userStore = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            _signInManager = new Mock<SignInManager<User>>(_userManager.Object,new Mock<IHttpContextAccessor>().Object,new Mock<IUserClaimsPrincipalFactory<User>>().Object,null,null,null);
            _userService = new UserService(_userManager.Object, _signInManager.Object);
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

            services.AddTransient<UserService>();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // act
            var userController = serviceProvider.GetService<UserService>();

            // assert
            Assert.NotNull(userController);
        }

        [Fact]
        public async Task SignUp_ShouldThrowException_IfUserIsNull()
        {
            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.SignUpAsync(null));
        }

        [Fact]
        public async Task SignUp_ShouldCallUserManageCreate_Once()
        {
            // arrange
            _userManager.Reset();
            var user = new User();
            _userManager.Setup(m => m.CreateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // act
            await _userService.SignUpAsync(user);

            // assert
            _userManager.Verify(m => m.CreateAsync(user), Times.Once);

        }

        [Fact]
        public async Task SignUp_ShouldReturnsFalse_IfFailToCreateUser()
        {
            // arrange
            var user = new User();
            _userManager.Reset();
            _userManager.Setup(m => m.CreateAsync(user)).ReturnsAsync(IdentityResult.Failed());

            // act
            bool actual = await _userService.SignUpAsync(user);

            // assert
            Assert.False(actual);
        }

        [Fact]
        public async Task SignUp_ShouldReturnTrue_IfCreateUserSuccess()
        {
            // arrange
            var user = new User();
            _userManager.Reset();
            _userManager.Setup(m => m.CreateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // act
            bool actual = await _userService.SignUpAsync(user);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTrue_IfSuccess()
        {
            // arrange
            var model = new LoginViewModel();
            _signInManager.Setup(m => m.PasswordSignInAsync(model.UserName, model.Password, false, false)).ReturnsAsync(SignInResult.Success);

            // act
            bool actual = await _userService.LoginAsync(model.UserName, model.Password);

            // assert
            Assert.True(actual);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_IfLoginFail()
        {
            // arrange
            var model = new LoginViewModel();
            _signInManager.Setup(m => m.PasswordSignInAsync(model.UserName, model.Password, false, false)).ReturnsAsync(SignInResult.Failed);

            // act
            bool actual = await _userService.LoginAsync(model.UserName, model.Password);

            // assert
            Assert.False(actual);
        }
    }
}
