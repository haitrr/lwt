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
    using Lwt.Models;

    /// <summary>
    /// a.
    /// </summary>
    public class LanguageService : ILanguageService
    {
        private readonly IValidator<Language> languageValidator;

        private readonly IMapper<Guid, LanguageCreateModel, Language> languageCreateMapper;

        private readonly ILanguageRepository languageRepository;

        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageService"/> class.
        /// </summary>
        /// <param name="languageValidator">languageValidator.</param>
        /// <param name="languageCreateMapper">languageCreateMapper.</param>
        /// <param name="languageRepository">languageRepository.</param>
        /// <param name="userRepository"> the user repository.</param>
        public LanguageService(
            IValidator<Language> languageValidator,
            IMapper<Guid, LanguageCreateModel, Language> languageCreateMapper,
            ILanguageRepository languageRepository,
            IUserRepository userRepository)
        {
            this.languageValidator = languageValidator;
            this.languageCreateMapper = languageCreateMapper;
            this.languageRepository = languageRepository;
            this.userRepository = userRepository;
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

            await this.languageRepository.AddAsync(language);

            return language.Id;
        }

        /// <inheritdoc/>
        public async Task<ICollection<Language>> GetByUserAsync(Guid userId)
        {
            User user = await this.userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new BadRequestException("User not found.");
            }

            return await this.languageRepository.GetByUserAsync(userId);
        }
    }
}