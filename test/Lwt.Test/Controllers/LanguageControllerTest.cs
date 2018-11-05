namespace Lwt.Test.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Lwt.Controllers;
    using Lwt.Interfaces;
    using Lwt.Models;

    using Microsoft.AspNetCore.Mvc;

    using Moq;

    using Xunit;

    /// <inheritdoc />
    /// <summary>
    /// test.
    /// </summary>
    public class LanguageControllerTest : IDisposable
    {
        private readonly Mock<ILanguageService> languageService;

        private readonly LanguageController languageController;

        private readonly Mock<IAuthenticationHelper> authenticationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageControllerTest"/> class.
        /// test.
        /// </summary>
        public LanguageControllerTest()
        {
            this.languageService = new Mock<ILanguageService>();
            this.authenticationHelper = new Mock<IAuthenticationHelper>();

            this.languageController = new LanguageController(
                this.languageService.Object,
                this.authenticationHelper.Object);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="LanguageControllerTest"/> class.
        /// destructor
        /// </summary>
        ~LanguageControllerTest()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_ShouldReturnOkWithId_IfSuccess()
        {
            // arrange
            Guid id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.languageController.User)).Returns(userId);
            var languageCreateModel = new LanguageCreateModel();
            this.languageService.Setup(s => s.CreateAsync(userId, languageCreateModel)).ReturnsAsync(id);

            // act
            IActionResult actual = await this.languageController.CreateAsync(languageCreateModel);

            // assert
            var actualObj = Assert.IsType<OkObjectResult>(actual);
            var value = Assert.IsType<Guid>(actualObj.Value);
            Assert.True(value == id);
        }

        /// <inheritdoc />
        /// <summary>
        ///  dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  dispose.
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.languageController?.Dispose();
            }
        }
    }
}