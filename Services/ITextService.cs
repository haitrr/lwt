using System.Collections.Generic;
using LWT.Models;


namespace LWT.Services
{
    public interface ITextService
    {
        bool IsExist(int id);
        void Add(Text text);
        void Delete(Text text);
        Text GetByID(int id);
        void Update(Text text);
        List<Text> GetAll();
        void Parse(int id);
        IEnumerable<Term> GetAllTerm(int id);

    }
}