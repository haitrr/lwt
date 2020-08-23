namespace Lwt.Test.Services
{
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// test user password changer class.
    /// </summary>
    public class UserPasswordChangerTest
    {
        private readonly IUserPasswordChanger userPasswordChanger;
        private readonly Mock<IUserRepository> userRepositoryMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPasswordChangerTest"/> class.
        /// </summary>
        public UserPasswordChangerTest()
        {
            this.userRepositoryMock = new Mock<IUserRepository>();
            this.userPasswordChanger = new UserPasswordChanger(this.userRepositoryMock.Object);
        }

        /// <summary>
        /// test di injection.
        /// </summary>
        [Fact]
        public void ShouldGetResolvedByDi()
        {
            var dependencyResolverHelper = new DependencyResolverHelper();
            Assert.IsType<UserPasswordChanger>(dependencyResolverHelper.GetService<IUserPasswordChanger>());
        }

        /// <summary>
        /// change password should return result from repository.
        /// </summary>
        /// <param name="expected">the result from repo.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChangePasswordAsyncShouldReturnResult(bool expected)
        {
            this.userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(It.IsNotNull<User>());
            this.userRepositoryMock.Setup(
                    r => r.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expected);

            bool result = await this.userPasswordChanger.ChangePasswordAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>());

            Assert.Equal(expected, result);
        }
    }
}