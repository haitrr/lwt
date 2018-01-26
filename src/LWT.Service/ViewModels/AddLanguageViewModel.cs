using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LWT.Service.ViewModels
{
    public class AddLanguageViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
