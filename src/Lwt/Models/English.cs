namespace Lwt.Models
{
    using Lwt.Interfaces;

    /// <summary>
    /// English language.
    /// </summary>
    public class English : ILanguage
    {
        /// <inheritdoc/>
        public string Name { get; set; } = "English";

        /// <inheritdoc/>
        public string[] SplitText(string text)
        {
            return text.Split(" ");
        }
    }
}