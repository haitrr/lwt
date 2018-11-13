namespace Lwt.Models
{
    using System;

    /// <summary>
    /// the text filters.
    /// </summary>
    public class TextFilter
    {
        /// <summary>
        /// Gets or sets the language of the text.
        /// </summary>
        public Guid? LanguageId { get; set; }
    }
}