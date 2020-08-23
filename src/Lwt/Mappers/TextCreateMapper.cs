namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;
    using Lwt.ViewModels;

    /// <summary>
    /// a.
    /// </summary>
    public class TextCreateMapper : BaseMapper<TextCreateModel, int, Text>
    {
        /// <inheritdoc/>
        public override Text Map(TextCreateModel from, int from2, Text result)
        {
            result.Title = from.Title;
            result.Content = from.Content;
            result.CreatorId = from2;
            result.LanguageCode = from.LanguageCode;

            return result;
        }
    }
}