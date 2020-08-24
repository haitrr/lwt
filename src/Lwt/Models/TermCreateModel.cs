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
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets term meaning.
        /// </summary>
        public string Meaning { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets term learning level.
        /// </summary>
        public LearningLevel LearningLevel { get; set; } = null!;

        /// <summary>
        /// Gets or sets term's language.
        /// </summary>
        public LanguageCode LanguageCode { get; set; } = null!;
    }
}