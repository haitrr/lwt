namespace Lwt.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// a.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }
    }
}
