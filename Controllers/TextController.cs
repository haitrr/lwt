using System;
using Microsoft.AspNetCore.Mvc;

namespace LWT.Controllers
{
    public class TextController:Controller
    {
        // The index page show list of all text in current language
        public IActionResult Index()
        {
            // Todo: Implement
            return View();
        }
    }
}