namespace Lwt.Models
{
    /// <summary>
    /// a.
    /// </summary>
    public class Text : Entity
    {
        /// <summary>
        /// table name.
        /// </summary>
        public const string TableName = "texts";

        /// <summary>
        /// Gets or sets creator id.
        /// </summary>
        public int CreatorId { get; set; }

        /// <summary>
        /// Gets or sets bookmark by user.
        /// </summary>
        public ulong? Bookmark { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets language.
        /// </summary>
        public LanguageCode LanguageCode { get; set; } = null!;
    }
}