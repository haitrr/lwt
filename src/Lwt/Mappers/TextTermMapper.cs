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

            return result;
        }
    }
}