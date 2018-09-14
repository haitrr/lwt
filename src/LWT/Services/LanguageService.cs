using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lwt.Exceptions;
using Lwt.Interfaces;
using Lwt.Models;

namespace Lwt.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IValidator<Language> _languageValidator;
        private readonly IMapper<Guid, LanguageCreateModel, Language> _languageCreateMapper;
        private readonly ILanguageRepository _languageRepository;
        private readonly ITransaction _transaction;

        public LanguageService(IValidator<Language> languageValidator,
            IMapper<Guid, LanguageCreateModel, Language> languageCreateMapper, ILanguageRepository languageRepository,
            ITransaction transaction)
        {
            _languageValidator = languageValidator;
            _languageCreateMapper = languageCreateMapper;
            _languageRepository = languageRepository;
            _transaction = transaction;
        }

        public async Task<Guid> CreateAsync(Guid creatorId, LanguageCreateModel languageCreateModel)
        {
            Language language = _languageCreateMapper.Map(creatorId, languageCreateModel);
            ValidationResult validationResult = _languageValidator.Validate(language);

            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
            }

            _languageRepository.Add(language);
            await _transaction.Commit();
            return language.Id;
        }
    }
}