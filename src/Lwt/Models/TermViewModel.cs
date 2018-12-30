namespace Lwt.Models
{
    using System;

    /// <summary>
    /// term view model.
    /// </summary>
    public class TermViewModel
    {
        /// <summary>
        /// Gets or sets content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets meaning.
        /// </summary>
        public string Meaning { get; set; }

        /// <summary>
        /// Gets or sets learning level.
        /// </summary>
        public TermLearningLevel LearningLevel { get; set; }

        /// <summary>
        /// Gets or sets term's id.
        /// </summary>
        public Guid Id { get; set; }
    }
}