using System.Collections.Generic;
using LWT.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        SelectList GetSelectList();
    }
}