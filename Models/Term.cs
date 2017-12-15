using System;
using System.Collections.Generic;

namespace LWT.Models
{
    public class Term
    {
        public Term()
        {
            this.ContainingTexts = new List<TextTerm>();
        }
        public int ID { get; set; }
        public string Content { get; set; }
        public int Level { get; set; }

        public string Meaning { get; set; }

        public Language Language { get; set; }

        public DateTime CreateTime { get; set;}

        // Many to many with text reference
        public ICollection<TextTerm> ContainingTexts { get; set; }

    }
}