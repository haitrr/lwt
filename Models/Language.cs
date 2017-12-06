using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LWT.Models
{
    public class Language
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int TextSize { get; set; }

    }
}