using System;
using System.ComponentModel.DataAnnotations;

namespace Lwt.Models
{
    public abstract class Entity
    {
        [Key]
        public Guid Id { get; set; }
    }
}
