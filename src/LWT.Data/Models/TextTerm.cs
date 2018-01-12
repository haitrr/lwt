
using System;
using System.ComponentModel.DataAnnotations;

namespace LWT.Data.Models
{
    public class TextTerm
    {
        [Key]
        public Guid TextId { get; set; }
        [Key]
        public Guid TermId { get; set; }
        public Text Text { get; set; }
        public Term Term { get; set; }

    }
}