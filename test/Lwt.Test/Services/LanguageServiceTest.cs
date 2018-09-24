namespace Lwt.Test.Services
{
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

    /// <summary>
    /// a.
    /// </summary>
    public class LanguageServiceTest
    {
        private readonly LanguageService languageService;
        private readonly Mock<IValidator<Language>> languageValidator;
        private readonly Mock<IMapper<Guid, LanguageCreateModel, Language>> languageCreateMapper;
        private readonly Mock<ILanguageRepository> languageRepository;
        private readonly Mock<ITransaction> transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageServiceTest"/> class.
        /// </summary>
        public LanguageServiceTest()
        {
            this.languageRepository = new Mock<ILanguageRepository>();
            this.languageCreateMapper = new Mock<IMapper<Guid, LanguageCreateModel, Language>>();
            this.languageValidator = new Mock<IValidator<Language>>();
            this.transaction = new Mock<ITransaction>();

            this.languageService = new LanguageService(
                this.languageValidator.Object,
                this.languageCreateMapper.Object,
                this.languageRepository.Object,
                this.transaction.Object);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateAsync_ThrowException_IfModelNotValid()
        {
            // arrange
            var language = new Language();
            var languageCreateModel = new LanguageCreateModel();
            Guid userId = Guid.NewGuid();
            this.languageCreateMapper.Setup(m => m.Map(userId, languageCreateModel)).Returns(language);
            var validateResult = new Mock<ValidationResult>();
            validateResult.Setup(v => v.IsValid).Returns(false);
            validateResult.Object.Errors.Add(new ValidationFailure("propertyName", "error"));
            this.languageValidator.Setup(v => v.Validate(language)).Returns(validateResult.Object);

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                this.languageService.CreateAsync(userId, languageCreateModel));
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateAsync_ShouldReturnId_IfModelNotValid()
        {
            // arrange
            var language = new Language();
            var languageCreateModel = new LanguageCreateModel();
            Guid userId = Guid.NewGuid();
            this.languageCreateMapper.Setup(m => m.Map(userId, languageCreateModel)).Returns(language);
            var validateResult = new Mock<ValidationResult>();
            validateResult.Setup(v => v.IsValid).Returns(true);
            this.languageValidator.Setup(v => v.Validate(language)).Returns(validateResult.Object);

            // act
            Guid actual = await this.languageService.CreateAsync(userId, languageCreateModel);

            // assert
            Assert.Equal(language.Id, actual);
        }
    }
}