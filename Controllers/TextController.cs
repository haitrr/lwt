using System;
using LWT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LWT.Controllers
{
    public class TextController : Controller
    {
        private readonly LWTContext _context;

        public TextController(LWTContext context)
        {
            _context = context;
        }
        // The index page show list of all text in current language
        public IActionResult Index()
        {
            var texts = _context.Texts.Include(text => text.Language);
            ListTextViewModel listTextViewModel = new ListTextViewModel()
            {
                Texts = texts.ToList()
            };
            return View(listTextViewModel);
        }

        public IActionResult Detail(int id)
        {
            var selectedText = _context.Texts.Include(text => text.Language).FirstOrDefault(text => text.ID == id);
            return View(selectedText);
        }


        // GET /Text/Edit/Id
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Text selectedText = _context.Texts.Include(text => text.Language).FirstOrDefault(text => text.ID == id);

            EditTextViewModel editTextViewModel = new EditTextViewModel()
            {
                Text = selectedText,
                // All the languages in database
                Languages = new SelectList(
                    _context.Languages,
                    "ID",
                    "Name"
                    )
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
            text.Language = _context.Languages.FirstOrDefault(language => language.ID == languageID);

            // Save change
            try
            {
                _context.Update(text);
                _context.SaveChanges();
            }
            catch(DbUpdateConcurrencyException)
            {
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}