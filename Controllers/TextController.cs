using System;
using LWT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using LWT.Services;

namespace LWT.Controllers
{
    public class TextController : Controller
    {
        private readonly ITextService _textService;
        private readonly ILanguageService _languageService;

        public TextController(ITextService textService,ILanguageService languageService)
        {
            _textService = textService;
            _languageService = languageService;
        }
        // The index page show list of all text in current language
        public IActionResult Index()
        {
            var texts = _textService.GetAll();
            ListTextViewModel listTextViewModel = new ListTextViewModel()
            {
                Texts = texts
            };
            return View(listTextViewModel);
        }

        public IActionResult Detail(int id)
        {
            var selectedText = _textService.GetByID(id);
            return View(selectedText);
        }


        // GET /Text/Edit/Id
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Text selectedText = _textService.GetByID(id);

            EditTextViewModel editTextViewModel = new EditTextViewModel()
            {
                Text = selectedText,
                // All the languages in database
                Languages = _languageService.GetSelectList()
        };
            return View(editTextViewModel);
        }

        // POST /Text/Edit/ID

        [HttpPost]
        public IActionResult Edit(int id, [Bind("ID,Title,Content")] Text text)
        {
            if (id != text.ID)
            {
                return NotFound();
            }
            // Get the Language Id from request
            int languageID = Int32.Parse(Request.Form["Language"].Single());
            text.Language = _languageService.GetByID(languageID);
            _textService.Update(text);
            return RedirectToAction(nameof(Detail),new {id = id});
        }
        // GET /Text/Add
        [HttpGet]
        public IActionResult Add()
        {
            EditTextViewModel editTextViewModel = new EditTextViewModel()
            {
                Languages = _languageService.GetSelectList()
            };
            return View(editTextViewModel);
        }

        // POST /Text/Add
        [HttpPost]
        public IActionResult Add([Bind("Title,Content")] Text text)
        {
            // Get the Language Id from request
            int languageID = Int32.Parse(Request.Form["Language"].Single());
            text.Language = _languageService.GetByID(languageID);
            _textService.Add(text);
            return RedirectToAction(nameof(Detail),new {id = text.ID});
        }
    }
}