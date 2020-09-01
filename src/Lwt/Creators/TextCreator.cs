namespace Lwt.Creators
{
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
        private readonly ISqlTextRepository textRepository;
        private readonly IDbTransaction dbTransaction;

        public TextCreator(
            IValidator<Text> textValidator,
            ISqlTextRepository textRepository,
            IDbTransaction dbTransaction)
        {
            this.textValidator = textValidator;
            this.textRepository = textRepository;
            this.dbTransaction = dbTransaction;
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
            await this.dbTransaction.CommitAsync();

            return text.Id;
        }
    }
}