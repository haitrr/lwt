namespace Lwt.ViewModels
{
    using Lwt.Models;

    /// <summary>
    /// text view model.
    /// </summary>
    public class TextViewModel
    {
        /// <summary>
        /// Gets or sets x.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets language.
        /// </summary>
        public LanguageCode LanguageCode { get; set; } = null!;

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        public long TermCount { get; set; }

        public int ProcessedIndex { get; set; }

        public ulong? Bookmark { get; set; }
    }
}