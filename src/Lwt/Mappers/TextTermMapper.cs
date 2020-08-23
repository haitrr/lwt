namespace Lwt.Mappers
{
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Services;

    public class TextTermMapper : BaseMapper<TextTerm, TermReadModel>
    {
        private readonly ILanguageHelper languageHelper;

        public TextTermMapper(ILanguageHelper languageHelper)
        {
            this.languageHelper = languageHelper;
        }

        public override TermReadModel Map(TextTerm from, TermReadModel result)
        {
            ILanguage language = this.languageHelper.GetLanguage(from.Text.LanguageCode);
            result.Content = from.Content;
            result.Id = from.Term?.Id;
            result.Index = from.Index;
            result.LearningLevel = from.Term == null
                ? language.ShouldSkip(from.Content) ? LearningLevel.Skipped : LearningLevel.Unknown
                : from.Term.LearningLevel;
            return result;
        }
    }
}