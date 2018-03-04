using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LWT.Data.Models
{
    public class Text : Entity
    {
        [Required] public string Content { get; set; }

        public Language Language { get; set; }
        public ICollection<TextTerm> TextTerms { get; set; }
    }
}