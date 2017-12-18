using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LWT.Models
{
    public class Text
    {
        public Text()
        {
            this.Terms = new List<TextTerm>();
        }
        public int ID { get; set; }
        [Required]
        public string Content { get; set; }

        [Required]
        public string Title { get; set; }
        public string AudioURL { get; set; }
        public string SourceURL { get; set; }

        [Required]
        public Language Language { get; set; }
        // Many to many with term reference
        public IList<TextTerm> Terms { get; set; }
    }
}