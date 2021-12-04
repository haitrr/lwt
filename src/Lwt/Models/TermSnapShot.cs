namespace Lwt.Models;

public record TermSnapShot(string Content, string LanguageCode, string LearningLevel)
{
    public string Content { get; set; } = Content;

    public string LanguageCode { get; set; } = LanguageCode;

    public string LearningLevel { get; set; } = LearningLevel;
}