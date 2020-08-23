namespace Lwt.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// the text read model.
    /// </summary>
    public class TextReadModel
    {
        /// <summary>
        /// Gets or sets the text title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text's language.
        /// </summary>
        public LanguageCode LanguageCode { get; set; } = null!;

        /// <summary>
        /// Gets or sets user bookmark.
        /// </summary>
        public ulong? Bookmark { get; set; }

        /// <summary>
        /// Gets or sets text id.
        /// </summary>
        public int Id { get; set; }
    }
}