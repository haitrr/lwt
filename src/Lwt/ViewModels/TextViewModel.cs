namespace Lwt.ViewModels
{
    using System;

    using Lwt.Models;

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
        /// Gets or sets language.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }
    }
}