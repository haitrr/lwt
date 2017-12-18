using System.Collections.Generic;
using LWT.Models;

namespace LWT.Services
{
    public class TextTermService : ITextTermService
    {
        private readonly LWTContext _context;
        public TextTermService(LWTContext context)
        {
            _context = context;
        }
        public void Add(TextTerm textTerm)
        {
            _context.TextTerm.Add(textTerm);
            _context.SaveChanges();
        }

        public void Delete(TextTerm textTerm)
        {
            _context.TextTerm.Remove(textTerm);
            _context.SaveChanges();
        }

        public void DeleteRange(IEnumerable<TextTerm> textTerms)
        {
            _context.TextTerm.RemoveRange(textTerms);
            _context.SaveChanges();
        }
    }
}