namespace Lwt.Models
{
    using System;

    /// <summary>
    /// the language.
    /// </summary>
    public class Language : Entity
    {
        /// <summary>
        /// Gets or sets a.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets b.
        /// </summary>
        public Guid CreatorId { get; set; }

        /// <summary>
        /// Gets or sets the delimiter pattern.
        /// </summary>
        public string DelimiterPattern { get; set; }
    }
}