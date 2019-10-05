namespace Lwt.ViewModels
{
    using Lwt.Models;

    /// <summary>
    /// c.
    /// </summary>
    public class TextCreateModel
    {
        /// <summary>
        /// Gets or sets z.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets x.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public Language Language { get; set; }
    }
}