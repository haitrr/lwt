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

    /// <inheritdoc />
    public class TextCreator : ITextCreator
    {
        private readonly IValidator<Text> textValidator;
        private readonly ITextSeparator textSeparator;
        private readonly ISqlTextRepository textRepository;
        private readonly ISqlTermRepository termRepository;
        private readonly ITextTermRepository textTermRepository;
        private readonly IDbTransaction dbTransaction;

        public TextCreator(
            IValidator<Text> textValidator,
            ITextSeparator textSeparator,
            ISqlTextRepository textRepository,
            IDbTransaction dbTransaction,
            ISqlTermRepository termRepository,
            ITextTermRepository textTermRepository)
        {
            this.textValidator = textValidator;
            this.textSeparator = textSeparator;
            this.textRepository = textRepository;
            this.dbTransaction = dbTransaction;
            this.termRepository = termRepository;
            this.textTermRepository = textTermRepository;
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

            this.textRepository.Add(text);
            List<string> words = this.textSeparator.SeparateText(text.Content, text.LanguageCode)
                .ToList();
            var textTerms = new List<TextTerm>();

            for (var i = 0; i < words.Count; i += 1)
            {
                string word = words[i];

                Term? term = await this.termRepository.TryGetByUserAndLanguageAndContentAsync(
                    text.CreatorId,
                    text.LanguageCode,
                    word);

                textTerms.Add(new TextTerm() { TermId = term?.Id, Content = word, Index = i, Text = text });
            }

            this.textTermRepository.BulkAdd(textTerms);
            await this.dbTransaction.CommitAsync();
            return text.Id;
        }
    }
}