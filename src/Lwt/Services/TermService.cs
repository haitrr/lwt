namespace Lwt.Services;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

    private readonly IMapper<Term, TermViewModel> termViewMapper;
    private readonly IMapper<Term, TermMeaningDto> termMeaningMapper;
    private readonly ITermEditor termEditor;
    private readonly ITermCreator termCreator;

    public TermService(
        ISqlTermRepository termRepository,
        IMapper<Term, TermViewModel> termViewMapper,
        IMapper<Term, TermMeaningDto> termMeaningMapper,
        ITermEditor termEditor, ITermCreator termCreator)
    {
        this.termRepository = termRepository;
        this.termViewMapper = termViewMapper;
        this.termMeaningMapper = termMeaningMapper;
        this.termEditor = termEditor;
        this.termCreator = termCreator;
    }

    /// <inheritdoc/>
    public Task<int> CreateAsync(Term term)
    {
        return this.termCreator.CreateAsync(term);
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