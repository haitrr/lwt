namespace Lwt.Interfaces
{
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
        /// split the text.
        /// </summary>
        /// <param name="text">the text.</param>
        /// <returns>the segmented words.</returns>
        string[] SplitText(string text);
    }
}