namespace Lwt.Test.Controllers
{
    using System;
    using System.Collections.Generic;
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

        private readonly Mock<IMapper<Language, LanguageViewModel>> languageViewModelMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageControllerTest"/> class.
        /// test.
        /// </summary>
        public LanguageControllerTest()
        {
            this.languageService = new Mock<ILanguageService>();
            this.authenticationHelper = new Mock<IAuthenticationHelper>();
            this.languageViewModelMapper = new Mock<IMapper<Language, LanguageViewModel>>();

            this.languageController = new LanguageController(
                this.languageService.Object,
                this.authenticationHelper.Object,
                this.languageViewModelMapper.Object);
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
        /// make sure get async work.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_ShouldWork()
        {
            // arrange
            Guid userId = Guid.NewGuid();
            var languages = new List<Language>();
            var viewModels = new List<LanguageViewModel>();
            this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.languageController.User)).Returns(userId);
            this.languageService.Setup(h => h.GetByUserAsync(userId)).ReturnsAsync(languages);
            this.languageViewModelMapper.Setup(m => m.Map(languages)).Returns(viewModels);

            // assert
            IActionResult actual = await this.languageController.GetAsync();

            // assert
            var objectResult = Assert.IsType<OkObjectResult>(actual);
            Assert.Equal(viewModels, objectResult.Value);
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