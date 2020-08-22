namespace Lwt.Mappers
{
    using System;

    using Lwt.Models;
    using Lwt.Services;
    using Lwt.ViewModels;

    /// <summary>
    /// a.
    /// </summary>
    public class TextCreateMapper : BaseMapper<TextCreateModel, Guid, Text>
    {
        /// <inheritdoc/>
        public override Text Map(TextCreateModel from, Guid from2, Text result)
        {
            result.Title = from.Title;
            result.Content = from.Content;
            result.CreatorId = from2;
            result.LanguageCode = from.LanguageCode;

            return result;
        }
    }
}