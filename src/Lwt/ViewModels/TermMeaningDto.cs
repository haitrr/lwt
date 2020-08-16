namespace Lwt.ViewModels
{
    using System;

    /// <summary>
    /// term meaning dto.
    /// </summary>
    public class TermMeaningDto
    {
        /// <summary>
        /// Gets or sets term id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets term meaning.
        /// </summary>
        public string Meaning { get; set; } = string.Empty;
    }
}