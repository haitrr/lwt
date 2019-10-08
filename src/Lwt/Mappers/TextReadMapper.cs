namespace Lwt.Mappers {
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
        
        public TextReadMapper(
            ISkippedWordRemover skippedWordRemover,
            ILanguageHelper languageHelper,
            ITermRepository termRepository) {
            this.skippedWordRemover = skippedWordRemover;
            this.languageHelper = languageHelper;
            this.termRepository = termRepository;
        }

        /// <inheritdoc/>
        public override async Task<TextReadModel> MapAsync(Text source, TextReadModel result)
        {
            result.Title = source.Title;
            result.Language = source.Language;
            result.Bookmark = source.Bookmark;
            result.Id = source.Id;

            var termViewModels = new List<TermReadModel>();
            ILanguage language = this.languageHelper.GetLanguage(source.Language);
            IEnumerable<string> notSkippedTerms = this.skippedWordRemover.RemoveSkippedWords(source.Words, source.Language);
            IDictionary<string, Term> termDict = await this.termRepository.GetManyAsync(
                source.CreatorId,
                language.Id,
                notSkippedTerms.ToHashSet());

            foreach (string word in source.Words)
            {
                if (language.ShouldSkip(word))
                {
                    termViewModels.Add(new TermReadModel { Content = word, LearningLevel = TermLearningLevel.Skipped });
                    continue;
                }

                TermReadModel viewModel;
                string normalizedWord = language.Normalize(word);

                if (!termDict.ContainsKey(normalizedWord))
                {
                    viewModel = new TermReadModel { Content = word, LearningLevel = TermLearningLevel.UnKnow };
                }
                else
                {
                    Term term = termDict[normalizedWord];
                    viewModel = new TermReadModel
                    {
                        Id = term.Id,
                        Content = word,
                        LearningLevel = term.LearningLevel,
                        Meaning = term.Meaning,
                    };
                }

                termViewModels.Add(viewModel);
            }

            result.Terms = termViewModels;

            return result;
        }
    }
}