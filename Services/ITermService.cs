using LWT.Models;
using System.Collections.Generic;


namespace LWT.Services
{
    public interface ITermService
    {
        void Add(Term term);
        void Delete(Term term);
        void Update(Term term);
        bool IsExist(int id);
        Term GetByID(int id);
        List<Term> GetAll();
        Term GetByContentAndLanguage(string content, Language language);
    }
}