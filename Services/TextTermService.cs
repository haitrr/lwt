using LWT.Models;

namespace LWT.Services
{
    public class TextTermService : ITextTermService
    {
        private readonly LWTContext _context;
        public void Add(TextTerm textTerm)
        {
            _context.Add(textTerm);
            _context.SaveChanges();
        }
    }
}