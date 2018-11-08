namespace Lwt.Test.Services
{
    using System;
    using System.Collections.Generic;
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

        private readonly Mock<IUserRepository> userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageServiceTest"/> class.
        /// </summary>
        public LanguageServiceTest()
        {
            this.languageRepository = new Mock<ILanguageRepository>();
            this.languageCreateMapper = new Mock<IMapper<Guid, LanguageCreateModel, Language>>();
            this.languageValidator = new Mock<IValidator<Language>>();
            this.transaction = new Mock<ITransaction>();
            this.userRepository = new Mock<IUserRepository>();

            this.languageService = new LanguageService(
                this.languageValidator.Object,
                this.languageCreateMapper.Object,
                this.languageRepository.Object,
                this.transaction.Object,
                this.userRepository.Object);
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
            await Assert.ThrowsAsync<BadRequestException>(
                () => this.languageService.CreateAsync(userId, languageCreateModel));
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

        /// <summary>
        /// get by user should return bad request if the requested user not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetByUserAsync_ShouldReturnBadRequest_IfUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            this.userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<BadRequestException>(() => this.languageService.GetByUserAsync(userId));
        }

        /// <summary>
        /// get by user should return correct language.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetByUserAsync_ShouldWork_IfUserFound()
        {
            Guid userId = Guid.NewGuid();
            var user = new User();
            this.userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            var languages = new List<Language>();
            this.languageRepository.Setup(r => r.GetByUserAsync(userId)).ReturnsAsync(languages);

            ICollection<Language> actual = await this.languageService.GetByUserAsync(userId);
            Assert.Equal(languages, actual);
        }
    }
}