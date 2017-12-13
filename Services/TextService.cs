using System.Collections.Generic;
using System.Linq;
using LWT.Models;
using Microsoft.EntityFrameworkCore;

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
            return _context.Text.Include(text => text.Language).ToList();
        }

        // get the text coresponding to given id
        public Text GetByID(int id)
        {
            return _context.Text.Include(text => text.Language).FirstOrDefault(text => text.ID == id);
        }

        // check if a text with given id exist
        public bool IsExist(int id)
        {
            return _context.Text.Any(text => text.ID == id);
        }

        // remove a text from database
        public void Delete(Text text)
        {
            if (IsExist(text.ID))
            {
                _context.Text.Remove(text);
                _context.SaveChanges();
            }
        }

        // update a text
        public void Update(Text text)
        {
            if(IsExist(text.ID))
            {
                _context.Text.Update(text);
                _context.SaveChanges();
            }
        }
    }
}