namespace Lwt.Models;

public record TermCreatedLogData(string LanguageCode, string Content, string LearningLevel)
{
    public string LanguageCode = LanguageCode;
    public string Content = Content;
    public string LearningLevel = LearningLevel;
}