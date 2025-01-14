using Lwt.Clients;

namespace Lwt.Models;

using System.Linq;
using System.Text.RegularExpressions;
using Lwt.Interfaces;

/// <summary>
/// Japanese language.
/// </summary>
public class Japanese : ILanguage
{
    private readonly IJapaneseSegmenterClient japaneseSegmenterClient;

    public Japanese(IJapaneseSegmenterClient japaneseSegmenterClient)
    {
        this.japaneseSegmenterClient = japaneseSegmenterClient;
    }

    /// <inheritdoc/>
    public string Name => "Japanese";

    /// <inheritdoc />
    public string SpeakCode => "ja-JP";

    /// <inheritdoc />
    public LanguageCode Code => LanguageCode.JAPANESE;

    /// <inheritdoc />
    public bool ShouldSkip(string term)
    {
        return !Regex.IsMatch(
            term,
            @"^[\u3040-\u309f\u30a0-\u30ff\uff00-\uff9f\u4e00-\u9faf\u3400-\u4dbf]+$");
    }

    /// <inheritdoc />
    public string[] SplitText(string text)
    {
        return this.japaneseSegmenterClient.CutAsync(text).GetAwaiter().GetResult().ToArray();
    }

    /// <inheritdoc />
    public string Normalize(string word)
    {
        return word;
    }
}