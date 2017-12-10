using System.Collections.Generic;
using System.Linq;
using LWT.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LWT.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly LWTContext _context;
        public LanguageService(LWTContext context)
        {
            _context = context;
        }
        public void Add(Language language)
        {
            _context.Language.Add(language);
            _context.SaveChanges();
        }

        public List<Language> GetAll()
        {
            return _context.Language.ToList();
        }

        public Language GetByID(int id)
        {
            return _context.Language.FirstOrDefault(language => language.ID == id);
        }

        public SelectList GetSelectList()
        {
            return new SelectList(
                    GetAll(),
                    "ID",
                    "Name"
                    );

        }

        public bool IsExist(int id)
        {
            return _context.Language.Any(language => language.ID == id);
        }

        public void Remove(Language language)
        {
            // Remove if the language exist
            if (IsExist(language.ID))
            {
                _context.Language.Remove(language);
            }
        }

        public void Update(Language language)
        {
            // Update if the language exist
            if (IsExist(language.ID))
            {
                _context.Language.Update(language);
            }
        }
    }
}