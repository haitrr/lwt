using System.Collections.Generic;

namespace LWT.Data.Models
{
    public class Term : Entity
    {
        public string Content { get; set; }
        public Language Language { get; set; }
        public ICollection<TextTerm> TextTerms { get; set; }
    }
}