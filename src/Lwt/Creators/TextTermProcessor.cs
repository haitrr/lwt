namespace Lwt.Creators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Repositories;

    public class TextTermProcessor : ITextTermProcessor
    {
        private readonly ITextSeparator textSeparator;
        private readonly ISqlTermRepository termRepository;
        private readonly ITextTermRepository textTermRepository;

        public TextTermProcessor(
            ITextSeparator textSeparator,
            ISqlTermRepository termRepository,
            ITextTermRepository textTermRepository)
        {
            this.textSeparator = textSeparator;
            this.termRepository = termRepository;
            this.textTermRepository = textTermRepository;
        }

        public async Task ProcessTextTermAsync(Text text)
        {
            this.textTermRepository.DeleteByTextId(text.Id);
            List<string> words = this.textSeparator.SeparateText(text.Content, text.LanguageCode)
                .ToList();
            var textTerms = new List<TextTerm>();

            for (var i = 0; i < words.Count; i += 1)
            {
                string word = words[i];

                Term? term = await this.termRepository.TryGetByUserAndLanguageAndContentAsync(
                    text.UserId,
                    text.LanguageCode,
                    word);

                textTerms.Add(new TextTerm() { TermId = term?.Id, Content = word, Index = i, TextId = text.Id });
            }

            this.textTermRepository.BulkAdd(textTerms);
        }
    }
}