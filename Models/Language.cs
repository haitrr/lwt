using System;

namespace LWT.Models
{
    public class Language
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int TextSize { get; set; }

        public string[] wordSplitter { get; set; }
    }
}