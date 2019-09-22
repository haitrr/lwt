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
        public override Text Map(TextCreateModel from1, Guid from2, Text result)
        {
            result.Title = from1.Title;
            result.Content = from1.Content;
            result.CreatorId = from2;
            result.Language = from1.Language;

            return result;
        }
    }
}