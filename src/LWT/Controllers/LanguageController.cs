using System;
using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lwt.Controllers
{
    [Route("/api/language")]
    public class LanguageController : Controller
    {
        private readonly ILanguageService _languageService;
        private readonly IAuthenticationHelper _authenticationHelper;

        public LanguageController(ILanguageService languageService, IAuthenticationHelper authenticationHelper)
        {
            _languageService = languageService;
            _authenticationHelper = authenticationHelper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(LanguageCreateModel languageCreateModel)
        {
            Guid userId = _authenticationHelper.GetLoggedInUser(User);
            Guid id = await _languageService.CreateAsync(userId, languageCreateModel);
            return Ok(id);
        }
    }
}