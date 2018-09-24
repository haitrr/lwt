namespace Lwt.Mappers
{
    using System;
    using LWT.Models;
    using Lwt.Services;
    using Lwt.ViewModels;

    /// <summary>
    /// a.
    /// </summary>
    public class TextCreateMapper : BaseMapper<TextCreateModel, Guid, Text>
    {
        /// <inheritdoc/>
        public override Text Map(TextCreateModel createModel, Guid creatorId, Text text)
        {
            text.Title = createModel.Title;
            text.Content = createModel.Content;
            text.LanguageId = createModel.LanguageId;
            text.UserId = creatorId;
            return text;
        }
    }
}