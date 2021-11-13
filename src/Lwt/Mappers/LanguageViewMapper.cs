namespace Lwt.Mappers;

using Lwt.Interfaces;
using Lwt.Models;
using Lwt.Services;

/// <summary>
/// language view mapper.
/// </summary>
public class LanguageViewMapper : BaseMapper<ILanguage, LanguageViewModel>
{
    /// <inheritdoc/>
    public override LanguageViewModel Map(ILanguage from, LanguageViewModel result)
    {
        result.Name = from.Name;
        result.Code = from.Code;
        result.SpeakCode = from.SpeakCode;

        return result;
    }
}