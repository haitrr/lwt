namespace Lwt.Models
{
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
        public IEnumerable<TermViewModel> Terms { get; set; }
    }
}