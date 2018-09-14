using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lwt.Exceptions;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.Services;
using Moq;
using Xunit;

namespace Lwt.Test.Services
{
    public class LanguageServiceTest
    {
        private readonly LanguageService _languageService;
        private readonly Mock<IValidator<Language>> _languageValidator;
        private readonly Mock<IMapper<Guid, LanguageCreateModel, Language>> _languageCreateMapper;
        private readonly Mock<ILanguageRepository> _languageRepository;
        private readonly Mock<ITransaction> _transaction;

        public LanguageServiceTest()
        {
            _languageRepository = new Mock<ILanguageRepository>();
            _languageCreateMapper = new Mock<IMapper<Guid, LanguageCreateModel, Language>>();
            _languageValidator = new Mock<IValidator<Language>>();
            _transaction = new Mock<ITransaction>();

            _languageService = new LanguageService(_languageValidator.Object, _languageCreateMapper.Object,
                _languageRepository.Object, _transaction.Object);
        }

        [Fact]
        public async Task CreateAsync_ThrowException_IfModelNotValid()
        {
            // arrange
            var language = new Language();
            var languageCreateModel = new LanguageCreateModel();
            Guid userId = Guid.NewGuid();
            _languageCreateMapper.Setup(m => m.Map(userId, languageCreateModel)).Returns(language);
            var validateResult = new Mock<ValidationResult>();
            validateResult.Setup(v => v.IsValid).Returns(false);
            validateResult.Object.Errors.Add(new ValidationFailure("propertyName","error"));
            _languageValidator.Setup(v => v.Validate(language)).Returns(validateResult.Object);

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _languageService.CreateAsync(userId, languageCreateModel));
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnId_IfModelNotValid()
        {
            // arrange
            var language = new Language();
            var languageCreateModel = new LanguageCreateModel();
            Guid userId = Guid.NewGuid();
            _languageCreateMapper.Setup(m => m.Map(userId, languageCreateModel)).Returns(language);
            var validateResult = new Mock<ValidationResult>();
            validateResult.Setup(v => v.IsValid).Returns(true);
            _languageValidator.Setup(v => v.Validate(language)).Returns(validateResult.Object);

            //act
            Guid actual = await _languageService.CreateAsync(userId, languageCreateModel);

            // assert
            Assert.Equal(language.Id, actual);
        }
    }
}