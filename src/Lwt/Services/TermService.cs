namespace Lwt.Services
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Lwt.Extensions;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.ViewModels;

    /// <summary>
    /// term service.
    /// </summary>
    public class TermService : ITermService
    {
        private readonly ISqlTermRepository termRepository;
        private readonly IDbTransaction dbTransaction;

        private readonly IMapper<TermEditModel, Term> termEditMapper;
        private readonly IMapper<Term, TermViewModel> termViewMapper;
        private readonly IMapper<Term, TermMeaningDto> termMeaningMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermService"/> class.
        /// </summary>
        /// <param name="termRepository">the term repository.</param>
        /// <param name="termEditMapper">the term edit mapper.</param>
        /// <param name="termViewMapper">term view mapper.</param>
        /// <param name="termMeaningMapper">term meaning mapper.</param>
        /// <param name="dbTransaction">db transaction.</param>
        public TermService(
            ISqlTermRepository termRepository,
            IMapper<TermEditModel, Term> termEditMapper,
            IMapper<Term, TermViewModel> termViewMapper,
            IMapper<Term, TermMeaningDto> termMeaningMapper,
            IDbTransaction dbTransaction)
        {
            this.termRepository = termRepository;
            this.termEditMapper = termEditMapper;
            this.termViewMapper = termViewMapper;
            this.termMeaningMapper = termMeaningMapper;
            this.dbTransaction = dbTransaction;
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(Term term)
        {
            this.termRepository.Add(term);
            await this.dbTransaction.CommitAsync();

            return term.Id;
        }

        /// <inheritdoc/>
        public async Task EditAsync(TermEditModel termEditModel, Guid termId, Guid userId)
        {
            Term current = await this.termRepository.GetUserTermAsync(termId, userId);

            Term edited = this.termEditMapper.Map(termEditModel, current);
            this.termRepository.Update(edited);
            await this.dbTransaction.CommitAsync();
        }

        /// <inheritdoc />
        public async Task<TermViewModel> GetAsync(Guid userId, Guid termId)
        {
            Term term = await this.termRepository.GetUserTermAsync(termId, userId);

            return this.termViewMapper.Map(term);
        }

        /// <inheritdoc />
        public Task<int> CountAsync(Guid userId, TermFilter termFilter)
        {
            Expression<Func<Term, bool>> filter = termFilter.ToExpression();
            filter = filter.And(term => term.CreatorId == userId);
            return this.termRepository.CountAsync(filter);
        }

        /// <inheritdoc />
        public async Task<TermMeaningDto> GetMeaningAsync(Guid userId, Guid termId)
        {
            Term term = await this.termRepository.GetUserTermAsync(termId, userId);

            return this.termMeaningMapper.Map(term);
        }
    }
}