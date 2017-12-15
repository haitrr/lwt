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
            _context.Add(textTerm);
            _context.SaveChanges();
        }
    }
}