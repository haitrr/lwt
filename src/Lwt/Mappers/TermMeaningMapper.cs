namespace Lwt.Mappers;

using Lwt.Models;
using Lwt.Services;
using Lwt.ViewModels;

/// <summary>
/// term meaning mapper.
/// </summary>
public class TermMeaningMapper : BaseMapper<Term, TermMeaningDto>
{
    /// <inheritdoc />
    public override TermMeaningDto Map(Term from, TermMeaningDto result)
    {
        result.Meaning = from.Meaning;
        result.Id = from.Id;
        return result;
    }
}