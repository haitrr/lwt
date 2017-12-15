using System.Collections.Generic;
using LWT.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LWT.Services
{
    public class TermService : ITermService
    {
        private readonly LWTContext _context;
        public TermService(LWTContext context)
        {
            _context = context;
        }
        public void Add(Term term)
        {
            _context.Term.Add(term);
            _context.SaveChanges();
        }

        public void Delete(Term term)
        {
            throw new System.NotImplementedException();
        }

        public List<Term> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public Term GetByContentAndLanguage(string content, Language language)
        {
            return _context.Term.SingleOrDefault(term => term.Content == content && term.Language == language);
        }

        public Term GetByID(int id)
        {
            throw new System.NotImplementedException();
        }

        public bool IsExist(int id)
        {
            throw new System.NotImplementedException();
        }

        public void Update(Term term)
        {
            _context.Update(term);
            _context.SaveChanges();
        }
    }
}