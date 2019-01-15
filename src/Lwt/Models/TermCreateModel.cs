namespace Lwt.Models
{
    /// <summary>
    /// term create model.
    /// </summary>
    public class TermCreateModel
    {
        /// <summary>
        /// Gets or sets term content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets term meaning.
        /// </summary>
        public string Meaning { get; set; }

        /// <summary>
        /// Gets or sets term learning level.
        /// </summary>
        public TermLearningLevel LearningLevel { get; set; }

        /// <summary>
        /// Gets or sets term's language.
        /// </summary>
        public Language Language { get; set; }
    }
}