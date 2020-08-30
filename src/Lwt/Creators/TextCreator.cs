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
    using Microsoft.EntityFrameworkCore.Storage;

    /// <inheritdoc />
    public class TextCreator : ITextCreator
    {
        private readonly IValidator<Text> textValidator;
        private readonly ISqlTextRepository textRepository;
        private readonly IDbTransaction dbTransaction;
        private readonly ITextTermProcessor textTermProcessor;

        public TextCreator(
            IValidator<Text> textValidator,
            ISqlTextRepository textRepository,
            IDbTransaction dbTransaction,
            ITextTermProcessor textTermProcessor)
        {
            this.textValidator = textValidator;
            this.textRepository = textRepository;
            this.dbTransaction = dbTransaction;
            this.textTermProcessor = textTermProcessor;
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

                await this.textTermProcessor.ProcessTextTermAsync(text);
                transaction.Commit();
            }

            return text.Id;
        }
    }
}