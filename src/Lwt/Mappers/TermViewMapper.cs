namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// term view mapper.
    /// </summary>
    public class TermViewMapper : BaseMapper<Term, TermViewModel>
    {
        /// <inheritdoc />
        public override TermViewModel Map(Term from, TermViewModel result)
        {
            result.Id = from.Id;
            result.Content = from.Content;
            result.Meaning = from.Meaning;
            result.LearningLevel = from.LearningLevel;
            return result;
        }
    }
}