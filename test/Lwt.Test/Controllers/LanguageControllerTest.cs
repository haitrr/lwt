namespace Lwt.Test.Controllers;

using System;
using System.Collections.Generic;
using Lwt.Controllers;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

/// <inheritdoc />
/// <summary>
/// test language controller.
/// </summary>
public class LanguageControllerTest : IDisposable
{
    private readonly LanguageController languageController;

    private readonly Mock<IAuthenticationHelper> authenticationHelper;

    private readonly Mock<ILanguageHelper> languageHelper;

    private readonly Mock<IMapper<ILanguage, LanguageViewModel>> languageViewModelMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageControllerTest"/> class.
    /// test.
    /// </summary>
    public LanguageControllerTest()
    {
        this.authenticationHelper = new Mock<IAuthenticationHelper>();
        this.languageViewModelMapper = new Mock<IMapper<ILanguage, LanguageViewModel>>();
        this.languageHelper = new Mock<ILanguageHelper>();

        this.languageController = new LanguageController(
            this.languageViewModelMapper.Object,
            this.languageHelper.Object);
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
    [Fact]
    public void GetAsyncShouldWork()
    {
        // arrange
        var userId = 1;
        var languages = new List<ILanguage>();
        var viewModels = new List<LanguageViewModel>();
        this.authenticationHelper.Setup(h => h.GetLoggedInUserId()).Returns(userId);
        this.languageHelper.Setup(h => h.GetAllLanguages()).Returns(languages);
        this.languageViewModelMapper.Setup(m => m.Map(languages)).Returns(viewModels);

        // assert
        IActionResult actual = this.languageController.GetAsync();

        // assert
        var objectResult = Assert.IsType<OkObjectResult>(actual);
        Assert.Equal(viewModels, objectResult.Value);
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
            this.languageController.Dispose();
        }
    }
}