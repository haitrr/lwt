namespace Lwt.Models
{
    /// <summary>
    /// language setting.
    /// </summary>
    public class LanguageSetting : Entity
    {
        public const string TableName = "language_settings";

        /// <summary>
        ///  Gets or sets dictionary language.
        /// </summary>
        public string DictionaryLanguage { get; set; } = string.Empty;

        public LanguageCode LanguageCode { get; set; } = null!;
    }
}