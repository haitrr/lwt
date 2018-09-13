using System;

namespace Lwt.Models
{
    public class Language : Entity
    {
        public string Name { get; set; }
        public Guid CreatorId { get; set; }
    }
}