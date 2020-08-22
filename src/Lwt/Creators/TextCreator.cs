namespace Lwt.Creators
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class TextCreator : ITextCreator
    {
        private readonly IValidator<Text> textValidator;
        private readonly ITextSeparator textSeparator;
        private readonly ITextRepository textRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextCreator"/> class.
        /// </summary>
        /// <param name="textValidator">the text validator.</param>
        /// <param name="textSeparator">the text splitter.</param>
        /// <param name="textRepository">the repo.</param>
        public TextCreator(
            IValidator<Text> textValidator,
            ITextSeparator textSeparator,
            ITextRepository textRepository)
        {
            this.textValidator = textValidator;
            this.textSeparator = textSeparator;
            this.textRepository = textRepository;
        }

        /// <inheritdoc />
        public async Task CreateAsync(Text text)
        {
            ValidationResult validationResult = this.textValidator.Validate(text);

            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
            }

            text.Words = this.textSeparator.SeparateText(text.Content, text.LanguageCode).ToList();
            await this.textRepository.AddAsync(text);
        }
    }
}