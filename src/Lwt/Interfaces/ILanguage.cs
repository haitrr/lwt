namespace Lwt.Interfaces
{
    using Lwt.Models;

    /// <summary>
    /// Languages.
    /// </summary>
    public interface ILanguage
    {
        /// <summary>
        /// Gets or sets the language's name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the enum.
        /// </summary>
        Language Id { get; set; }

        /// <summary>
        /// decide if a term should be skipped.
        /// </summary>
        /// <param name="term">the term.</param>
        /// <returns>should skip the term or not.</returns>
        bool ShouldSkip(string term);

        /// <summary>
        /// split the text.
        /// </summary>
        /// <param name="text">the text.</param>
        /// <returns>the segmented words.</returns>
        string[] SplitText(string text);

        /// <summary>
        /// normalize a string into language.
        /// </summary>
        /// <param name="word">the word.</param>
        /// <returns>normalized word.</returns>
        string Normalize(string word);
    }
}