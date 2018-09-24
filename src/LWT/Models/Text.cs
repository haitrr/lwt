namespace LWT.Models
{
    using System;
    using Lwt.Models;

    /// <summary>
    /// a.
    /// </summary>
    public class Text : Entity
    {
        /// <summary>
        /// Gets or sets UserId.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets Language id.
        /// </summary>
        public Guid LanguageId { get; set; }
    }
}