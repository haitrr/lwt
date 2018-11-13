namespace Lwt.Models
{
    using System.Collections.Generic;

    using Lwt.ViewModels;

    /// <summary>
    /// the text list with pagination.
    /// </summary>
    public class TextList
    {
        /// <summary>
        /// Gets or sets the total number of texts.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the text in the current page.
        /// </summary>
        public IEnumerable<TextViewModel> Items { get; set; }
    }
}