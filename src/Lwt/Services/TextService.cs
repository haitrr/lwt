namespace Lwt.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentValidation;
    using FluentValidation.Results;

    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;

    using LWT.Models;

    /// <summary>
    /// a.
    /// </summary>
    public class TextService : ITextService
    {
        private readonly ITextRepository textRepository;

        private readonly ITransaction transaction;

        private readonly IValidator<Text> textValidator;

        private readonly IMapper<TextEditModel, Text> textEditMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextService"/> class.
        /// </summary>
        /// <param name="textRepository">textRepository.</param>
        /// <param name="transaction">transaction.</param>
        /// <param name="textEditMapper">textEditMapper.</param>
        /// <param name="textValidator">textValidator.</param>
        public TextService(
            ITextRepository textRepository,
            ITransaction transaction,
            IMapper<TextEditModel, Text> textEditMapper,
            IValidator<Text> textValidator)
        {
            this.textRepository = textRepository;
            this.transaction = transaction;
            this.textEditMapper = textEditMapper;
            this.textValidator = textValidator;
        }

        /// <inheritdoc/>
        public Task CreateAsync(Text text)
        {
            ValidationResult validationResult = this.textValidator.Validate(text);

            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
            }

            this.textRepository.Add(text);

            return this.transaction.Commit();
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Text>> GetByUserAsync(Guid userId)
        {
            return this.textRepository.GetByUserAsync(userId);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, Guid userId)
        {
            Text text = await this.textRepository.GetByIdAsync(id);

            if (text.UserId == userId)
            {
                this.textRepository.DeleteById(text);
                await this.transaction.Commit();
            }
            else
            {
                throw new ForbiddenException("You don't have permission to delete this text");
            }
        }

        /// <inheritdoc/>
        public async Task EditAsync(Guid textId, Guid userId, TextEditModel editModel)
        {
            Text text = await this.textRepository.GetByIdAsync(textId);

            if (text.UserId == userId)
            {
                Text editedText = this.textEditMapper.Map(editModel, text);
                this.textRepository.Update(editedText);
                await this.transaction.Commit();
            }
            else
            {
                throw new ForbiddenException("You do not have permission to edit this text");
            }
        }
    }
}