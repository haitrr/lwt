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
            throw new System.NotImplementedException();
        }

        public List<Text> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public Text GetByID(int id)
        {
            throw new System.NotImplementedException();
        }

        public bool IsExist(int id)
        {
            throw new System.NotImplementedException();
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