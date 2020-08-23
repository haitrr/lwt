namespace Lwt.ViewModels
{
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
#pragma warning disable 8618
        public TextViewModel()
#pragma warning restore 8618
        {
            this.Counts = new Dictionary<LearningLevel, long>();
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
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets term counts by learning levels.
        /// </summary>
        // ReSharper disable once CollectionNeverQueried.Global
        public Dictionary<LearningLevel, long> Counts { get; set; }
    }
}