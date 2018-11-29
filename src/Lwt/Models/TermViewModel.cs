namespace Lwt.Models
{
    /// <summary>
    /// the term's view model.
    /// </summary>
    public class TermViewModel
    {
        /// <summary>
        /// Gets or sets the term content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the term's learning level.
        /// </summary>
        public TermLearningLevel LearningLevel { get; set; }

        /// <summary>
        /// Gets or sets the term's meaning.
        /// </summary>
        public string Mearning { get; set; }
    }
}