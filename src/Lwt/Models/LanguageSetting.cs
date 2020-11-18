namespace Lwt.Models
{
    /// <summary>
    /// language setting.
    /// </summary>
    public record LanguageSetting : Entity
    {
        public const string TableName = "language_settings";

        /// <summary>
        ///  Gets or sets dictionary language.
        /// </summary>
        public LanguageCode DictionaryLanguageCode { get; set; } = LanguageCode.ENGLISH;

        public LanguageCode LanguageCode { get; set; } = null!;
    }
}