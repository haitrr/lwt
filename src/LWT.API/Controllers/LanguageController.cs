using System.Threading.Tasks;
using LWT.Service.Interfaces;
using LWT.Service.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LWT.API.Controllers
{
    [Route("api/language")]
    public class LanguageController : Controller
    {
        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }


        [HttpPost]
        public async Task<IActionResult> Add(AddLanguageViewModel addLanguageViewModel)
        {
            if (ModelState.IsValid) return BadRequest();
            bool result = await _languageService.Add(addLanguageViewModel).ConfigureAwait(false);
            if (result) return Ok();
            return BadRequest();
        }
    }
}