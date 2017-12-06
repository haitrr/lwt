using System;
using System.Collections.Generic;

namespace LWT.Models
{
    public class Text
    {
        public int ID { get; set; }
        public string Content { get; set; } 

        public string Title { get; set; }
        public string AudioURL { get; set; }
        public string SourceURL { get; set; }

        public Language Language { get; set; }
        public HashSet<Tag> Tags { get; set; }
    }
}