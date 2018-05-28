using Lwt.Models;
using System;

namespace LWT.Models
{
    public class Text  : Entity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}