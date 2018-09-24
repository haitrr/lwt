namespace Lwt.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <summary>
    /// a.
    /// </summary>
    public class LanguageService : ILanguageService
    {
        private readonly IValidator<Language> languageValidator;
        private readonly IMapper<Guid, LanguageCreateModel, Language> languageCreateMapper;
        private readonly ILanguageRepository languageRepository;
        private readonly ITransaction transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageService"/> class.
        /// </summary>
        /// <param name="languageValidator">languageValidator.</param>
        /// <param name="languageCreateMapper">languageCreateMapper.</param>
        /// <param name="languageRepository">languageRepository.</param>
        /// <param name="transaction">transaction.</param>
        public LanguageService(
            IValidator<Language> languageValidator,
            IMapper<Guid, LanguageCreateModel, Language> languageCreateMapper,
            ILanguageRepository languageRepository,
            ITransaction transaction)
        {
            this.languageValidator = languageValidator;
            this.languageCreateMapper = languageCreateMapper;
            this.languageRepository = languageRepository;
            this.transaction = transaction;
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(Guid creatorId, LanguageCreateModel languageCreateModel)
        {
            Language language = this.languageCreateMapper.Map(creatorId, languageCreateModel);
            ValidationResult validationResult = this.languageValidator.Validate(language);

            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
            }

            this.languageRepository.Add(language);
            await this.transaction.Commit();
            return language.Id;
        }
    }
}