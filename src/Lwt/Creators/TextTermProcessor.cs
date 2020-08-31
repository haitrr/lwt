namespace Lwt.Creators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Repositories;
    using Lwt.Utilities;

    public class TextTermProcessor : ITextTermProcessor
    {
        private readonly ITextSeparator textSeparator;
        private readonly ISqlTermRepository termRepository;
        private readonly ITextTermRepository textTermRepository;
        private readonly ILanguageHelper languageHelper;
        private readonly ITextNormalizer textNormalizer;
        private readonly IDbTransaction dbTransaction;

        public TextTermProcessor(
            ITextSeparator textSeparator,
            ISqlTermRepository termRepository,
            ITextTermRepository textTermRepository,
            ILanguageHelper languageHelper,
            ITextNormalizer textNormalizer,
            IDbTransaction dbTransaction)
        {
            this.textSeparator = textSeparator;
            this.termRepository = termRepository;
            this.textTermRepository = textTermRepository;
            this.languageHelper = languageHelper;
            this.textNormalizer = textNormalizer;
            this.dbTransaction = dbTransaction;
        }

        public async Task ProcessTextTermAsync(Text text)
        {
            this.textTermRepository.DeleteByTextId(text.Id);
            List<string> words = this.textSeparator.SeparateText(text.Content, text.LanguageCode)
                .ToList();
            text.TermCount = words.Count;
            await this.dbTransaction.CommitAsync();
            ILanguage language = this.languageHelper.GetLanguage(text.LanguageCode);
            var textTerms = new List<TextTerm>();
            var termContentSet = new HashSet<string>();

            foreach (string word in words.Where(word => !language.ShouldSkip(word)))
            {
                termContentSet.Add(this.textNormalizer.Normalize(word, text.LanguageCode));
            }

            IEnumerable<Term> terms = await this.termRepository.TryGetManyByUserAndLanguageAndContentsAsync(
                text.UserId,
                text.LanguageCode,
                termContentSet);

            Dictionary<string, Term> termDict = terms.ToDictionary(t => t.Content, t => t);

            var newTerms = new List<Term>();

            for (var i = 0; i < words.Count; i += 1)
            {
                string word = words[i];
                string normalizedWord = this.textNormalizer.Normalize(word, text.LanguageCode);

                if (!termDict.ContainsKey(normalizedWord) && !language.ShouldSkip(normalizedWord))
                {
                    var term = new Term
                    {
                        LanguageCode = text.LanguageCode,
                        Content = normalizedWord,
                        UserId = text.UserId,
                        LearningLevel = LearningLevel.Unknown,
                        Meaning = string.Empty,
                    };
                    termDict[normalizedWord] = term;
                    newTerms.Add(term);
                }
            }

            this.termRepository.BulkInsert(newTerms);

            for (var i = 0; i < words.Count; i += 1)
            {
                string word = words[i];
                Term? term = null;
                string normalizedWord = this.textNormalizer.Normalize(word, text.LanguageCode);

                if (termDict.ContainsKey(normalizedWord))
                {
                    term = termDict[normalizedWord];
                }

                textTerms.Add(new TextTerm { TermId = term?.Id, Content = word, Index = i, TextId = text.Id });
            }

            this.textTermRepository.BulkInsert(textTerms);
        }
    }
}