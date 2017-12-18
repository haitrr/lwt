using LWT.Models;
using System.Collections.Generic;


namespace LWT.Services
{
    public interface ITextTermService
    {
        void Add(TextTerm textTerm);
        void Delete(TextTerm textTerm);
        void DeleteRange(IEnumerable<TextTerm> textTerms);
    }
}