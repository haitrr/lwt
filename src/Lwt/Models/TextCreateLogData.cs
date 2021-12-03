namespace Lwt.Models;

public record TextCreateLogData(string Title, string LanguageCode)
{
    public string Title { get; set; } = Title;
    public string LanguageCode { get; set; } = LanguageCode;
}