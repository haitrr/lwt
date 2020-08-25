namespace Lwt.Creators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Repositories;
    using Lwt.Utilities;

    /// <inheritdoc />
    public class TextCreator : ITextCreator
    {
        private readonly IValidator<Text> textValidator;
        private readonly ITextSeparator textSeparator;
        private readonly ISqlTextRepository textRepository;
        private readonly ISqlTermRepository termRepository;
        private readonly ITextTermRepository textTermRepository;
        private readonly IDbTransaction dbTransaction;
        private readonly ITextNormalizer textNormalizer;
        private readonly ILanguageHelper languageHelper;

        public TextCreator(
            IValidator<Text> textValidator,
            ITextSeparator textSeparator,
            ISqlTextRepository textRepository,
            IDbTransaction dbTransaction,
            ISqlTermRepository termRepository,
            ITextTermRepository textTermRepository,
            ILanguageHelper languageHelper,
            ITextNormalizer textNormalizer)
        {
            this.textValidator = textValidator;
            this.textSeparator = textSeparator;
            this.textRepository = textRepository;
            this.dbTransaction = dbTransaction;
            this.termRepository = termRepository;
            this.textTermRepository = textTermRepository;
            this.languageHelper = languageHelper;
            this.textNormalizer = textNormalizer;
        }

        /// <inheritdoc />
        public async Task<int> CreateAsync(Text text)
        {
            ValidationResult validationResult = this.textValidator.Validate(text);

            if (!validationResult.IsValid)
            {
                throw new BadRequestException(
                    validationResult.Errors.First()
                        .ErrorMessage);
            }

            ILanguage language = this.languageHelper.GetLanguage(text.LanguageCode);
            this.textRepository.Add(text);
            List<string> words = this.textSeparator.SeparateText(text.Content, text.LanguageCode)
                .ToList();
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

            for (var i = 0; i < words.Count; i += 1)
            {
                string word = words[i];

                termDict.TryGetValue(this.textNormalizer.Normalize(word, text.LanguageCode), out Term? term);

                textTerms.Add(new TextTerm { TermId = term?.Id, Content = word, Index = i, Text = text });
            }

            this.textTermRepository.BulkAdd(textTerms);
            await this.dbTransaction.CommitAsync();
            return text.Id;
        }
    }
}