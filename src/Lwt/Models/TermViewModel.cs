namespace Lwt.Models
{
    using System;

    /// <summary>
    /// the term's view model.
    /// </summary>
    public class TermViewModel
    {
        /// <summary>
        /// Gets or sets the term's id.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the term content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the term's meaning.
        /// </summary>
        public string Meaning { get; set; }

        /// <summary>
        /// Gets or sets the term's learning level.
        /// </summary>
        public TermLearningLevel LearningLevel { get; set; }
    }
}