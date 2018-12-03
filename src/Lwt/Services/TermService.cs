namespace Lwt.Services
{
    using System;
    using System.Threading.Tasks;

    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <summary>
    /// term service.
    /// </summary>
    public class TermService : ITermService
    {
        private readonly ITermRepository termRepository;

        private readonly IMapper<TermEditModel, Term> termEditMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermService"/> class.
        /// </summary>
        /// <param name="termRepository">the term repository.</param>
        /// <param name="termEditMapper">the term edit mapper.</param>
        public TermService(ITermRepository termRepository, IMapper<TermEditModel, Term> termEditMapper)
        {
            this.termRepository = termRepository;
            this.termEditMapper = termEditMapper;
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(Term term)
        {
            await this.termRepository.AddAsync(term);

            return term.Id;
        }

        /// <inheritdoc/>
        public async Task EditAsync(TermEditModel editModel, Guid termId, Guid userId)
        {
            Term current = await this.termRepository.GetByIdAsync(termId);

            if (current.CreatorId != userId)
            {
                throw new ForbiddenException("You don't have permission to edit this term.");
            }

            Term edited = this.termEditMapper.Map(editModel, current);
            await this.termRepository.UpdateAsync(edited);
        }
    }
}