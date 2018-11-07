namespace Lwt.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

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
        /// Gets or sets creator.
        /// </summary>
        public User Creator { get; set; }

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
        [ForeignKey(nameof(Language))]
        public Guid LanguageId { get; set; }

        /// <summary>
        /// Gets or sets language.
        /// </summary>
        public Language Language { get; set; }
    }
}