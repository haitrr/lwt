namespace Lwt.Models
{
    using System;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// the term.
    /// </summary>
    public class Term : Entity
    {
        /// <summary>
        /// table name.
        /// </summary>
        public const string TableName = "terms";

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the term's language.
        /// </summary>
        public LanguageCode LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the meaning.
        /// </summary>
        public string Meaning { get; set; } = string.Empty;

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