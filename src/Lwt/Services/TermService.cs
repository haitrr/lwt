namespace Lwt.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Lwt.Extensions;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Repositories;
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
        private readonly ITextTermRepository textTermRepository;

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
            IDbTransaction dbTransaction,
            ITextTermRepository textTermRepository)
        {
            this.termRepository = termRepository;
            this.termEditMapper = termEditMapper;
            this.termViewMapper = termViewMapper;
            this.termMeaningMapper = termMeaningMapper;
            this.dbTransaction = dbTransaction;
            this.textTermRepository = textTermRepository;
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(Term term)
        {
            this.termRepository.Add(term);

            // link the existing text term to the new term.
            IEnumerable<TextTerm> textTerms =
                await this.textTermRepository.GetByUserAndLanguageAndContentAsync(
                    term.UserId,
                    term.LanguageCode,
                    term.Content);

            foreach (TextTerm textTerm in textTerms)
            {
                textTerm.Term = term;
                this.textTermRepository.Update(textTerm);
            }

            await this.dbTransaction.CommitAsync();

            return term.Id;
        }

        /// <inheritdoc/>
        public async Task EditAsync(TermEditModel termEditModel, int termId, int userId)
        {
            Term current = await this.termRepository.GetUserTermAsync(termId, userId);

            Term edited = this.termEditMapper.Map(termEditModel, current);
            this.termRepository.Update(edited);
            await this.dbTransaction.CommitAsync();
        }

        /// <inheritdoc />
        public async Task<TermViewModel> GetAsync(int userId, int termId)
        {
            Term term = await this.termRepository.GetUserTermAsync(termId, userId);

            return this.termViewMapper.Map(term);
        }

        /// <inheritdoc />
        public Task<int> CountAsync(int userId, TermFilter termFilter)
        {
            Expression<Func<Term, bool>> filter = termFilter.ToExpression();
            filter = filter.And(term => term.UserId == userId);
            return this.termRepository.CountAsync(filter);
        }

        /// <inheritdoc />
        public async Task<TermMeaningDto> GetMeaningAsync(int userId, int termId)
        {
            Term term = await this.termRepository.GetUserTermAsync(termId, userId);

            return this.termMeaningMapper.Map(term);
        }
    }
}