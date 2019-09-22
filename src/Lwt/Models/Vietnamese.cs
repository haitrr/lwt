namespace Lwt.Models
{
  using Lwt.Interfaces;

  /// <summary>
  /// Vietnamese language.
  /// </summary>
  public class Vietnamese : ILanguage
  {
    /// <inheritdoc/>
    public string Name => "Vietnamese";

    /// <inheritdoc/>
    public string SpeakCode => "vi_VN";

    /// <inheritdoc/>
    public string Code => "vi";

    /// <inheritdoc/>
    public Language Id => Language.Vietnamese;

    /// <inheritdoc/>
    public bool ShouldSkip(string term)
    {
      throw new System.NotSupportedException();
    }

    /// <inheritdoc/>
    public string[] SplitText(string text)
    {
      throw new System.NotSupportedException();
    }

    /// <inheritdoc/>
    public string Normalize(string word)
    {
      throw new System.NotSupportedException();
    }
  }
}