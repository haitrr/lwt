namespace Lwt.Models
{
    /// <summary>
    /// text term.
    /// </summary>
    public record TextTerm : Entity
    {
        /// <summary>
        /// table name.
        /// </summary>
        public const string TableName = "text_terms";

        /// <summary>
        /// Gets or sets text id.
        /// </summary>
        public int TextId { get; set; }

        public Text Text { get; set; } = null!;

        /// <summary>
        /// Gets or sets term id.
        /// </summary>
        public int? TermId { get; set; }

        public Term? Term { get; set; }

        /// <summary>
        /// Gets or sets content.
        /// </summary>
        public string Content { get; set; } = null!;

        /// <summary>
        /// Gets or sets index.
        /// </summary>
        public int Index { get; set; }
    }
}