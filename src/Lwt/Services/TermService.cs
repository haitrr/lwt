namespace Lwt.Services;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Extensions;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.ViewModels;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// term service.
/// </summary>
public class TermService : ITermService
{
    private readonly ISqlTermRepository termRepository;
    private readonly IDbTransaction dbTransaction;

    private readonly IMapper<Term, TermViewModel> termViewMapper;
    private readonly IMapper<Term, TermMeaningDto> termMeaningMapper;
    private readonly ITermEditor termEditor;

    public TermService(
        ISqlTermRepository termRepository,
        IMapper<Term, TermViewModel> termViewMapper,
        IMapper<Term, TermMeaningDto> termMeaningMapper,
        IDbTransaction dbTransaction,
        ITermEditor termEditor)
    {
        this.termRepository = termRepository;
        this.termViewMapper = termViewMapper;
        this.termMeaningMapper = termMeaningMapper;
        this.dbTransaction = dbTransaction;
        this.termEditor = termEditor;
    }

    /// <inheritdoc/>
    public async Task<int> CreateAsync(Term term)
    {
        Term? existingTerm = await this.termRepository.TryGetByUserAndLanguageAndContentAsync(
            term.UserId,
            term.LanguageCode,
            term.Content);

        if (existingTerm != null)
        {
            throw new BadRequestException("Term has already exist.");
        }

        this.termRepository.Add(term);
        await this.dbTransaction.CommitAsync();

        return term.Id;
    }

    /// <inheritdoc/>
    public Task EditAsync(TermEditModel termEditModel, int termId, int userId)
    {
        return this.termEditor.EditAsync(termEditModel, termId, userId);
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
        Term term = await this.termRepository.Queryable()
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Id == termId)
            .Select(t => new Term { Meaning = t.Meaning, Id = t.Id }).FirstOrDefaultAsync();

        return this.termMeaningMapper.Map(term);
    }
}