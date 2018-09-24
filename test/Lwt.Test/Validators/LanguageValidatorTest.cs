namespace Lwt.Test.Validators
{
    using System;
    using FluentValidation.Results;
    using Lwt.Models;
    using Lwt.Validators;
    using Xunit;

    /// <summary>
    /// a.
    /// </summary>
    public class LanguageValidatorTest
    {
        private readonly LanguageValidator languageValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageValidatorTest"/> class.
        /// </summary>
        public LanguageValidatorTest()
        {
            this.languageValidator = new LanguageValidator();
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="name">name.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldReturnNotValid_IfModelNotValid(string name)
        {
            // arrange
            var language = new Language { Name = name };

            // act
            ValidationResult actual = this.languageValidator.Validate(language);

            // assert
            Assert.False(actual.IsValid);
        }

        /// <summary>
        /// a.
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
            var language = new Language { Name = name };

            // act
            ValidationResult actual = this.languageValidator.Validate(language);

            // assert
            Assert.False(actual.IsValid);
        }

        /// <summary>
        /// a.
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
            var language = new Language { Name = name, CreatorId = Guid.NewGuid() };

            // act
            ValidationResult actual = this.languageValidator.Validate(language);

            // assert
            Assert.True(actual.IsValid);
        }

        /// <summary>
        /// a.
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
    }
}