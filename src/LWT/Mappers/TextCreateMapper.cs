using System;
using Lwt.Services;
using Lwt.ViewModels;
using LWT.Models;

namespace Lwt.Mappers
{
    public class TextCreateMapper:BaseMapper<TextCreateModel,Guid,Text>
    {
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