namespace Lwt.Mappers
{
    using System;

    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// a.
    /// </summary>
    public class LanguageCreateMapper : BaseMapper<Guid, LanguageCreateModel, Language>
    {
        /// <inheritdoc/>
        public override Language Map(Guid creatorId, LanguageCreateModel createModel, Language language)
        {
            language.Name = createModel.Name;
            language.CreatorId = creatorId;

            return language;
        }
    }
}