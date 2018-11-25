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
        /// split the text.
        /// </summary>
        /// <param name="text">the text.</param>
        /// <returns>the segmented words.</returns>
        string[] SplitText(string text);
    }
}