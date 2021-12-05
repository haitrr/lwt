using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lwt.Models;
using Lwt.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lwt.Services;

public class TextCounter : ITextCounter
{
    private readonly ISqlTextRepository textRepository;

    public TextCounter(ISqlTextRepository textRepository)
    {
        this.textRepository = textRepository;
    }

    public Task<List<CountByLanguageCode>> CountByLanguageAsync(int userId,
        TextFilter filters)
    {
        IQueryable<Text> query = this.textRepository.Queryable()
            .Where(t => t.UserId == userId);

        if (filters.LanguageCode != null)
        {
            query = query.Where(t => t.LanguageCode == filters.LanguageCode);
        }

        if (filters.Title != null)
        {
            query = query.Where(
                t => t.Title.ToLower()
                    .Contains(filters.Title.ToLower()));
        }

        return query
            .GroupBy(t => t.LanguageCode)
            .Select(group => new CountByLanguageCode( group.Key,group.Sum(t => 1)))
            .ToListAsync();
    }
}

public record CountByLanguageCode(LanguageCode LanguageCode, long Count)
{
    public LanguageCode LanguageCode = LanguageCode;
    public long Count = Count;
}