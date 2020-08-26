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
    using Microsoft.EntityFrameworkCore.Storage;

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
            using (IDbContextTransaction transaction = this.dbTransaction.BeginTransaction())
            {
                this.textRepository.Add(text);
                await this.dbTransaction.CommitAsync();
                ValidationResult validationResult = this.textValidator.Validate(text);

                if (!validationResult.IsValid)
                {
                    throw new BadRequestException(
                        validationResult.Errors.First()
                            .ErrorMessage);
                }

                ILanguage language = this.languageHelper.GetLanguage(text.LanguageCode);
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

                await this.dbTransaction.CommitAsync();
                this.textTermRepository.BulkInsert(textTerms);

                transaction.Commit();
            }

            return text.Id;
        }
    }
}