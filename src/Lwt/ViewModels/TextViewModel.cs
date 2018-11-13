namespace Lwt.ViewModels
{
    using System;

    /// <summary>
    /// text view model.
    /// </summary>
    public class TextViewModel
    {
        /// <summary>
        /// Gets or sets x.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets language id.
        /// </summary>
        public Guid LanguageId { get; set; }
    }
}