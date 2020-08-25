namespace Lwt.Test.Utilities
{
    using System.Threading.Tasks;
    using Lwt.Exceptions;
    using Lwt.Models;
    using Lwt.Repositories;
    using Lwt.Utilities;
    using Moq;
    using Xunit;

    /// <summary>
    /// test user text getter.
    /// </summary>
    public class UserTextGetterTest
    {
        private readonly UserTextGetter userTextGetter;
        private readonly Mock<ISqlTextRepository> textRepositoryMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTextGetterTest"/> class.
        /// </summary>
        public UserTextGetterTest()
        {
            this.textRepositoryMock = new Mock<ISqlTextRepository>();
            this.userTextGetter = new UserTextGetter(this.textRepositoryMock.Object);
        }

        /// <summary>
        /// should not allow a user to access text of other users.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetUserTextAsyncShouldThrowForbidentIfNotCreator()
        {
            var text = new Text();
            var textId = 1;
            var userId = 1;
            this.textRepositoryMock.Setup(r => r.GetByIdAsync(textId))
                .ReturnsAsync(text);

            await Assert.ThrowsAsync<ForbiddenException>(() => this.userTextGetter.GetUserTextAsync(textId, userId));
        }

        /// <summary>
        /// should return the right text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetUserTextAsyncShouldReturnRightText()
        {
            var textId = 1;
            var userId = 1;
            var text = new Text { UserId = userId };
            this.textRepositoryMock.Setup(r => r.GetByIdAsync(textId))
                .ReturnsAsync(text);

            Text result = await this.userTextGetter.GetUserTextAsync(textId, userId);

            Assert.Equal(text, result);
        }
    }
}