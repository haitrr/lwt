namespace Lwt.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Utilities;

    /// <summary>
    /// text to text read model mapper.
    /// </summary>
    public class TextReadMapper : AbstractAsyncMapper<Text, TextReadModel>
    {
        private readonly ISkippedWordRemover skippedWordRemover;
        private readonly ILanguageHelper languageHelper;
        private readonly ITermRepository termRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextReadMapper"/> class.
        /// </summary>
        /// <param name="skippedWordRemover">the skipped word remover.</param>
        /// <param name="languageHelper">the language helper.</param>
        /// <param name="termRepository">the term repository.</param>
        public TextReadMapper(
            ISkippedWordRemover skippedWordRemover,
            ILanguageHelper languageHelper,
            ITermRepository termRepository)
        {
            this.skippedWordRemover = skippedWordRemover;
            this.languageHelper = languageHelper;
            this.termRepository = termRepository;
        }

        /// <inheritdoc/>
        public override async Task<TextReadModel> MapAsync(Text source, TextReadModel result)
        {
            result.Title = source.Title;
            result.LanguageCode = source.LanguageCode;
            result.Bookmark = source.Bookmark;
            result.Id = source.Id;

            var termViewModels = new List<TermReadModel>();
            ILanguage language = this.languageHelper.GetLanguage(source.LanguageCode);
            IEnumerable<string> notSkippedTerms = this.skippedWordRemover.RemoveSkippedWords(source.Words, source.LanguageCode);
            IEnumerable<string> normalizedTerms = notSkippedTerms.Select(t => language.Normalize(t));
            IDictionary<string, Term> termDict = await this.termRepository.GetManyAsync(
                source.CreatorId,
                language.Code,
                normalizedTerms.ToHashSet());

            foreach (string word in source.Words)
            {
                if (language.ShouldSkip(word))
                {
                    termViewModels.Add(new TermReadModel { Content = word, LearningLevel = LearningLevel.Skipped });
                    continue;
                }

                TermReadModel viewModel;
                string normalizedWord = language.Normalize(word);

                if (!termDict.ContainsKey(normalizedWord))
                {
                    viewModel = new TermReadModel { Content = word, LearningLevel = LearningLevel.Unknown };
                }
                else
                {
                    Term term = termDict[normalizedWord];
                    viewModel = new TermReadModel
                    {
                        Id = term.Id,
                        Content = word,
                        LearningLevel = term.LearningLevel,
                    };
                }

                termViewModels.Add(viewModel);
            }

            result.Terms = termViewModels;

            return result;
        }
    }
}