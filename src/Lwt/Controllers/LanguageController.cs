namespace Lwt.Controllers
{
    using System.Collections.Generic;

    using Lwt.Interfaces;
    using Lwt.Models;

    using Microsoft.AspNetCore.Mvc;

    /// <inheritdoc />
    /// <summary>
    /// a.
    /// </summary>
    [Route("/api/language")]
    public class LanguageController : Controller
    {
        private readonly ILanguageHelper languageHelper;

        private IMapper<ILanguage, LanguageViewModel> languageViewMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageController"/> class.
        /// </summary>
        /// <param name="languageViewMapper">language view model mapper.</param>
        /// <param name="languageHelper">language helper.</param>
        public LanguageController(
            IMapper<ILanguage, LanguageViewModel> languageViewMapper,
            ILanguageHelper languageHelper)
        {
            this.languageViewMapper = languageViewMapper;
            this.languageHelper = languageHelper;
        }

        /// <summary>
        /// get all language of the logged in user.
        /// </summary>
        /// <returns>list of the language view model.</returns>
        [HttpGet]
        public IActionResult GetAsync()
        {
            ICollection<ILanguage> languages = this.languageHelper.GetAllLanguages();

            ICollection<LanguageViewModel> languageViewModels = this.languageViewMapper.Map(languages);

            return this.Ok(languageViewModels);
        }
    }
}