using System;
using System.Threading.Tasks;
using Lwt.Controllers;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Lwt.Test.Controllers
{
    public class LanguageControllerTest
    {
        private readonly Mock<ILanguageService> _languageService;
        private readonly LanguageController _languageController;
        private readonly Mock<IAuthenticationHelper> _authenticationHelper;

        public LanguageControllerTest()
        {
            _languageService = new Mock<ILanguageService>();
            _authenticationHelper = new Mock<IAuthenticationHelper>();
            _languageController = new LanguageController(_languageService.Object, _authenticationHelper.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnOkWithId_IfSuccess()
        {
            // arrange
            Guid id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            _authenticationHelper.Setup(h => h.GetLoggedInUser(_languageController.User)).Returns(userId);
            var languageCreateModel = new LanguageCreateModel();
            _languageService.Setup(s => s.CreateAsync(userId, languageCreateModel)).ReturnsAsync(id);

            // act
            IActionResult actual = await _languageController.CreateAsync(languageCreateModel);

            // assert
            var actualObj = Assert.IsType<OkObjectResult>(actual);
            var value = Assert.IsType<Guid>(actualObj.Value);
            Assert.True(value == id);
        }
    }
}