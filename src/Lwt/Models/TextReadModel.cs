namespace Lwt.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// the text read model.
    /// </summary>
    public class TextReadModel
    {
        /// <summary>
        /// Gets or sets the text title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the terms of the text.
        /// </summary>
        public IEnumerable<TermReadModel> Terms { get; set; }

        /// <summary>
        /// Gets or sets terms information.
        /// </summary>
        public IDictionary<Guid, TermViewModel> TermsInformation { get; set; }

        /// <summary>
        /// Gets or sets the text's language.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Gets or sets user bookmark.
        /// </summary>
        public ulong? Bookmark { get; set; }

        /// <summary>
        /// Gets or sets text id.
        /// </summary>
        public Guid Id { get; set; }
    }
}