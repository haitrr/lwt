using System;
using Lwt.Models;
using Lwt.Services;

namespace Lwt.Mappers
{
    public class LanguageCreateMapper : BaseMapper<Guid, LanguageCreateModel, Language>
    {
        public override Language Map(Guid creatorId, LanguageCreateModel createModel, Language language)
        {
            language.Name = createModel.Name;
            language.CreatorId = creatorId;
            return language;
        }
    }
}