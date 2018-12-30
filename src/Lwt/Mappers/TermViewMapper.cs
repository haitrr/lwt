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
        public override TermViewModel Map(Term term, TermViewModel termViewModel)
        {
            termViewModel.Id = term.Id;
            termViewModel.Content = term.Content;
            termViewModel.Meaning = term.Meaning;
            termViewModel.LearningLevel = term.LearningLevel;
            return termViewModel;
        }
    }
}