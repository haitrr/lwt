namespace Lwt.Models
{
    /// <summary>
    /// the term's view model.
    /// </summary>
    public class TermReadModel
    {
        /// <summary>
        /// Gets or sets the term's id.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the term content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the term's learning level.
        /// </summary>
        public LearningLevel LearningLevel { get; set; } = null!;

        public int Index { get; set; }
    }
}