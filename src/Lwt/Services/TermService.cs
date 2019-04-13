namespace Lwt.Services
{
    using System;
    using System.Collections.Generic;
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
        private readonly IMapper<Term, TermViewModel> termViewMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermService"/> class.
        /// </summary>
        /// <param name="termRepository">the term repository.</param>
        /// <param name="termEditMapper">the term edit mapper.</param>
        /// <param name="termViewMapper">term view mapper.</param>
        public TermService(
            ITermRepository termRepository,
            IMapper<TermEditModel, Term> termEditMapper,
            IMapper<Term, TermViewModel> termViewMapper)
        {
            this.termRepository = termRepository;
            this.termEditMapper = termEditMapper;
            this.termViewMapper = termViewMapper;
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

        /// <inheritdoc />
        public async Task<TermViewModel> GetAsync(Guid userId, Guid termId)
        {
            Term term = await this.termRepository.GetByIdAsync(termId);

            if (term == null)
            {
                throw new NotFoundException("Term not found.");
            }

            if (term.CreatorId != userId)
            {
                throw new ForbiddenException("You don't have permission to access this term.");
            }

            return this.termViewMapper.Map(term);
        }

        /// <inheritdoc />
        public Task<ulong> CountAsync(Guid userId, TermFilter termFilter)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IEnumerable<TermViewModel>> SearchAsync(
            Guid userId,
            TermFilter termFilter,
            PaginationQuery paginationQuery)
        {
            throw new NotImplementedException();
        }
    }
}