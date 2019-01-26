namespace Lwt.Models
{
    /// <summary>
    /// Language view model.
    /// </summary>
    public class LanguageViewModel
    {
        /// <summary>
        /// Gets or sets language name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets language id.
        /// </summary>
        public Language Id { get; set; }

        /// <summary>
        /// Gets or sets the language's code.
        /// </summary>
        public string Code { get; set; }
    }
}