namespace Lwt.Mappers;

using Lwt.Models;
using Lwt.Services;

/// <summary>
/// mapper from text to text edit detail.
/// </summary>
public class TextEditDetailMapper : BaseMapper<Text, TextEditDetailModel>
{
    /// <inheritdoc />
    public override TextEditDetailModel Map(Text from, TextEditDetailModel result)
    {
        result.Id = from.Id;
        result.LanguageCode = from.LanguageCode;
        result.Title = from.Title;
        result.Content = from.Content;
        return result;
    }
}