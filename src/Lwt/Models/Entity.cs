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
        [MaxLength(36)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets when did the entity created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when did the entity last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}