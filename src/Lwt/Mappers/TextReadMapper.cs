namespace Lwt.Mappers
{
    using System.Collections.Generic;
    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// text to text read model mapper.
    /// </summary>
    public class TextReadMapper : BaseMapper<Text, TextReadModel>
    {
        /// <inheritdoc/>
        public override TextReadModel Map(Text source, TextReadModel result)
        {
            result.Title = source.Title;
            result.LanguageCode = source.LanguageCode;
            result.Bookmark = source.Bookmark;
            result.Id = source.Id;

            var termViewModels = new List<TermReadModel>();
            result.Terms = termViewModels;

            return result;
        }
    }
}