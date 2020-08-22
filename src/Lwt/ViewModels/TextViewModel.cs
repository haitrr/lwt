namespace Lwt.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Lwt.Models;

    /// <summary>
    /// text view model.
    /// </summary>
    public class TextViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextViewModel"/> class.
        /// </summary>
        public TextViewModel()
        {
            this.Counts = new Dictionary<TermLearningLevel, long>();
        }

        /// <summary>
        /// Gets or sets x.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets language.
        /// </summary>
        public LanguageCode LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets term counts by learning levels.
        /// </summary>
        public Dictionary<TermLearningLevel, long> Counts { get; }
    }
}