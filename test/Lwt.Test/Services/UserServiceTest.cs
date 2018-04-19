using System;
using System.Threading.Tasks;
using Lwt.DbContexts;
using Lwt.Models;
using Lwt.Services;
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

        public UserServiceTest()
        {
            _userManager = new Mock<UserManager<User>>(new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);
            _userService = new UserService(_userManager.Object);
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.SignUp(null));
        }

        [Fact]
        public async Task SignUp_ShouldCallUserManageCreate_Once()
        {
            // arrange
            _userManager.Reset();
            var user = new User();
            _userManager.Setup(m => m.CreateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // act
            await _userService.SignUp(user);

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
            bool actual = await _userService.SignUp(user);

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
            bool actual = await _userService.SignUp(user);

            // assert
            Assert.True(actual);
        }


    }
}
