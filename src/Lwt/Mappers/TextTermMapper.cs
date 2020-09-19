namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    public class TextTermMapper : BaseMapper<TextTerm, TermReadModel>
    {
        public override TermReadModel Map(TextTerm from, TermReadModel result)
        {
            result.Content = from.Content;
            result.Id = from.TermId;
            result.IndexFrom = from.IndexFrom;
            result.IndexTo = from.IndexTo;
            result.LearningLevel = from.Term == null ? LearningLevel.Skipped : from.Term.LearningLevel;

            if (result.LearningLevel != LearningLevel.Ignored && result.LearningLevel != LearningLevel.Skipped &&
                result.LearningLevel != LearningLevel.WellKnown)
            {
                result.Meaning = from.Term!.Meaning;
            }
            else
            {
                result.Meaning = null;
            }

            return result;
        }
    }
}