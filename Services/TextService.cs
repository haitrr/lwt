using System.Collections.Generic;
using System.Linq;
using LWT.Models;

namespace LWT.Services
{
    public class TextService : ITextService
    {
        private LWTContext _context;
        public TextService(LWTContext context)
        {
            _context = context;
        }
        // Add a text to database
        public void Add(Text text)
        {
            _context.Text.Add(text);
            _context.SaveChanges();
        }

        // Get all texts from database
        public List<Text> GetAll()
        {
            return _context.Text.ToList();
        }

        // get the text coresponding to given id
        public Text GetByID(int id)
        {
            return _context.Text.FirstOrDefault(text => text.ID == id);
        }

        // check if a text with given id exist
        public bool IsExist(int id)
        {
            return _context.Text.Any(text => text.ID == id);
        }

        public void Remove(Text text)
        {
            throw new System.NotImplementedException();
        }

        public void Update(Text text)
        {
            throw new System.NotImplementedException();
        }
    }
}