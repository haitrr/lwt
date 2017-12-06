using System;
using LWT.Models;
using Microsoft.AspNetCore.Mvc;

namespace LWT.Controllers
{
    public class TextController:Controller
    {
        private readonly LWTContext _context;

        public TextController(LWTContext context)
        {
            _context = context;
        }
        // The index page show list of all text in current language
        public IActionResult Index()
        {
            // Todo: Implement
            return View();
        }
    }
}