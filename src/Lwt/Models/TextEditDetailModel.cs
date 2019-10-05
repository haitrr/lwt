namespace Lwt.Models
{
    using System;

    /// <summary>
    /// edit detail dto.
    /// </summary>
    public class TextEditDetailModel
    {
        /// <summary>
        /// Gets or sets text id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the text's title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets text's content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets text's language.
        /// </summary>
        public Language Language { get; set; }
    }
}