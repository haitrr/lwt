namespace Lwt.Models;

public record TextReadLogData(int TextId, string LanguageCode)
{
    public int TextId = TextId;
    public string LanguageCode = LanguageCode;
}