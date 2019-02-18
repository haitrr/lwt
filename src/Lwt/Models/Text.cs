namespace Lwt.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// a.
    /// </summary>
    public class Text : Entity
    {
        /// <summary>
        /// Gets or sets creator id.
        /// </summary>
        public Guid CreatorId { get; set; }

        /// <summary>
        /// Gets or sets bookmark by user.
        /// </summary>
        public ulong? BookMark { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets language.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Gets or sets the words.
        /// </summary>
        public ICollection<string> Words { get; set; }
    }
}