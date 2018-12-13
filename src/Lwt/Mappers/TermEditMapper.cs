namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    /// <summary>
    /// the mapper to map the edit model and the current term to a edited term.
    /// </summary>
    public class TermEditMapper : BaseMapper<TermEditModel, Term>
    {
        /// <inheritdoc/>
        public override Term Map(TermEditModel editModel, Term term)
        {
            term.Meaning = editModel.Meaning;
            term.LearningLevel = editModel.LearningLevel;

            return term;
        }
    }
}