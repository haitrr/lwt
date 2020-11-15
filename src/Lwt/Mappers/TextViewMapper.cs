namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;
    using Lwt.ViewModels;

    /// <summary>
    /// text to view model mapper.
    /// </summary>
    public class TextViewMapper : BaseMapper<Text, TextViewModel>
    {
        /// <inheritdoc/>
        public override TextViewModel Map(Text from, TextViewModel result)
        {
            result.Title = from.Title;
            result.LanguageCode = from.LanguageCode;
            result.Id = from.Id;
            result.TermCount = from.TermCount;
            result.ProcessedTermCount = from.ProcessedTermCount;
            result.Bookmark = from.Bookmark;
            result.CreatedAt = from.CreatedDate;

            return result;
        }
    }
}