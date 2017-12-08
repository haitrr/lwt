using System.Collections.Generic;
using LWT.Models;


namespace LWT.Services
{
    public interface ILanguageService
    {
        void Add(Language language);
        void Remove(Language language);
        void Update(Language language);
        bool IsExist(int id);
        Language GetByID(int id);
        List<Language> GetAll();
    }
}