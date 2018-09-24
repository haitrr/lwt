namespace Lwt.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// a.
    /// </summary>
    [Route("/api/language")]
    public class LanguageController : Controller
    {
        private readonly ILanguageService languageService;
        private readonly IAuthenticationHelper authenticationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageController"/> class.
        /// </summary>
        /// <param name="languageService">a.</param>
        /// <param name="authenticationHelper">asdasd.</param>
        public LanguageController(ILanguageService languageService, IAuthenticationHelper authenticationHelper)
        {
            this.languageService = languageService;
            this.authenticationHelper = authenticationHelper;
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
    }
}