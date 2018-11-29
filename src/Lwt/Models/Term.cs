namespace Lwt.Models
{
    using System;

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
        /// Gets or sets the meaning.
        /// </summary>
        public string Mearning { get; set; }

        /// <summary>
        /// Gets or sets the creator's id.
        /// </summary>
        public Guid CreatorId { get; set; }

        /// <summary>
        /// Gets or sets the current learning level.
        /// </summary>
        public TermLearningLevel LearningLevel { get; set; }
    }
}