namespace Lwt.Models
{
    /// <summary>
    /// term view model.
    /// </summary>
    public class TermViewModel
    {
        /// <summary>
        /// Gets or sets meaning.
        /// </summary>
        public string Meaning { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets learning level.
        /// </summary>
        public LearningLevel LearningLevel { get; set; } = null!;
    }
}