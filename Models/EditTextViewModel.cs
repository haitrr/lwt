using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LWT.Models
{
    public class EditTextViewModel
    {
        public Text Text { get; set; }
        public SelectList Languages { get; set; }
    }
}