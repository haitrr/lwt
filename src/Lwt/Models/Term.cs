namespace Lwt.Models
{
    /// <summary>
    /// the term.
    /// </summary>
    public class Term : Entity
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the translate.
        /// </summary>
        public string Translate { get; set; }
    }
}