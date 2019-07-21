namespace Lwt.Models
{
    using Lwt.Interfaces;

    public class Vietnamese : ILanguage
    {
        public string Name => "Vietnamese";

        public string SpeakCode => "vi_VN";

        public string Code => "vi";

        public Language Id => Language.Vietnamese;

        public bool ShouldSkip(string term)
        {
            throw new System.NotImplementedException();
        }

        public string[] SplitText(string text)
        {
            throw new System.NotImplementedException();
        }

        public string Normalize(string word)
        {
            throw new System.NotImplementedException();
        }
    }
}