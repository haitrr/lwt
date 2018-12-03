namespace Lwt.Models
{
    /// <summary>
    /// term edit model.
    /// </summary>
    public class TermEditModel
    {
        /// <summary>
        /// Gets or sets meaning.
        /// </summary>
        public string Meaning { get; set; }

        /// <summary>
        /// Gets or sets content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets language.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Gets or sets the learning level.
        /// </summary>
        public TermLearningLevel LearningLevel { get; set; }
    }
}