using LWT.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace LWT.Controllers
{
    public class LanguageController : Controller
    {
        private LWTContext _context;
        public LanguageController(LWTContext context){
            this._context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            ListLanguageViewModel listLanguageViewModel = new ListLanguageViewModel()
            {
                Languages = _context.Language.ToList()
            };
            return View(listLanguageViewModel);
        }

        // GET /Language/Detail/ID
        public IActionResult Detail(int id)
        {
            Language selectedLanguage = _context.Language.FirstOrDefault(language => language.ID == id);
            return View(selectedLanguage);
        }
    }
}