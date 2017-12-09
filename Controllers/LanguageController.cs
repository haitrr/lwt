using LWT.Models;
using LWT.Services;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace LWT.Controllers
{
    public class LanguageController : Controller
    {
        private readonly ILanguageService _service;
        public LanguageController(ILanguageService service)
        {
            _service = service;
        }
        [HttpGet]
        public IActionResult Index()
        {
            ListLanguageViewModel listLanguageViewModel = new ListLanguageViewModel()
            {
                Languages = _service.GetAll()
            };
            return View(listLanguageViewModel);
        }
    }
}