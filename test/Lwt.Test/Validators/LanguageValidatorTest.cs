namespace Lwt.Test.Validators
{
    using System;
    using FluentValidation.Results;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Validators;
    using Moq;
    using Xunit;

    /// <summary>
    /// Test for language validator.
    /// </summary>
    public class LanguageValidatorTest
    {
        private readonly LanguageValidator languageValidator;
        private readonly Mock<IUserRepository> userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageValidatorTest"/> class.
        /// </summary>
        public LanguageValidatorTest()
        {
            this.userRepository = new Mock<IUserRepository>();
            this.languageValidator = new LanguageValidator(this.userRepository.Object);
        }

        /// <summary>
        /// Name should be valid.
        /// </summary>
        /// <param name="name">name.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("tieasdvgdctieasdvgdctieasdvgdcs")]
        [InlineData("ta;sdlkas;ldzkxc;kzxc;lieasdvgdctieasdvgdctieasdvgdcs")]
        public void Validate_ShouldReturnInValid_IfNameNotValid(string name)
        {
            // arrange
            var language = new Language { Name = name, CreatorId = this.GetValidUserId() };

            // act
            ValidationResult actual = this.languageValidator.Validate(language);

            // assert
            Assert.False(actual.IsValid);
        }

        /// <summary>
        /// Name should be valid.
        /// </summary>
        /// <param name="name">name.</param>
        [Theory]
        [InlineData("Eng")]
        [InlineData("English")]
        [InlineData("Vietnamese")]
        [InlineData("VietnameseEnglishLao")]
        public void Validate_ShouldReturnValid_IfNameValid(string name)
        {
            // arrange
            var language = new Language { Name = name, CreatorId = this.GetValidUserId() };

            // act
            ValidationResult actual = this.languageValidator.Validate(language);

            // assert
            Assert.True(actual.IsValid);
        }

        /// <summary>
        /// Creator id should not empty.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInvalid_IfGuidIsEmpty()
        {
            // arrange
            var language = new Language { Name = "asdl", CreatorId = Guid.Empty };

            // act
            ValidationResult actual = this.languageValidator.Validate(language);

            // assert
            Assert.False(actual.IsValid);
        }

        /// <summary>
        /// creator should exist.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInvalid_IfCreatorNotExist()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            var language = new Language { CreatorId = creatorId, Name = "asdd", Id = Guid.NewGuid() };
            this.userRepository.Setup(m => m.IsExistAsync(creatorId)).ReturnsAsync(false);

            // act
            ValidationResult actual = this.languageValidator.Validate(language);

            // assert
            Assert.False(actual.IsValid);
        }

        private Guid GetValidUserId()
        {
            Guid validId = Guid.NewGuid();
            this.userRepository.Setup(m => m.IsExistAsync(validId)).ReturnsAsync(true);
            return validId;
        }
    }
}