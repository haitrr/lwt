namespace Lwt.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Lwt.Interfaces;
    using Lwt.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <inheritdoc />
    /// <summary>
    /// a.
    /// </summary>
    [Route("/api/language")]
    public class LanguageController : Controller
    {
        private readonly ILanguageService languageService;

        private readonly IAuthenticationHelper authenticationHelper;

        private IMapper<Language, LanguageViewModel> languageViewMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageController"/> class.
        /// </summary>
        /// <param name="languageService">the language service.</param>
        /// <param name="authenticationHelper">the authentication helper.</param>
        /// <param name="languageViewMapper">language view model mapper.</param>
        public LanguageController(
            ILanguageService languageService,
            IAuthenticationHelper authenticationHelper,
            IMapper<Language, LanguageViewModel> languageViewMapper)
        {
            this.languageService = languageService;
            this.authenticationHelper = authenticationHelper;
            this.languageViewMapper = languageViewMapper;
        }

        /// <summary>
        /// aa.
        /// </summary>
        /// <param name="languageCreateModel">asdsad.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync(LanguageCreateModel languageCreateModel)
        {
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            Guid id = await this.languageService.CreateAsync(userId, languageCreateModel);

            return this.Ok(id);
        }

        /// <summary>
        /// get all language of the logged in user.
        /// </summary>
        /// <returns>list of the language view model.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAsync()
        {
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            ICollection<Language> languages = await this.languageService.GetByUserAsync(userId);
            ICollection<LanguageViewModel> languageViewModels = this.languageViewMapper.Map(languages);

            return this.Ok(languageViewModels);
        }
    }
}