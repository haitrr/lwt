using System;
using FluentValidation.Results;
using Lwt.Models;
using Lwt.Validators;
using Xunit;

namespace Lwt.Test.Validators
{
    public class LanguageValidatorTest
    {
        private readonly LanguageValidator _languageValidator;

        public LanguageValidatorTest()
        {
            _languageValidator = new LanguageValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_ShouldReturnNotValid_IfModelNotValid(string name)
        {
            // arrange
            var language = new Language {Name = name};
            // act
            ValidationResult actual = _languageValidator.Validate(language);

            // assert
            Assert.False(actual.IsValid);
        }


        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("tieasdvgdctieasdvgdctieasdvgdcs")]
        [InlineData("ta;sdlkas;ldzkxc;kzxc;lieasdvgdctieasdvgdctieasdvgdcs")]
        public void Validate_ShouldReturnInValid_IfNameNotValid(string name)
        {
            // arrange
            var language = new Language {Name = name};
            // act
            ValidationResult actual = _languageValidator.Validate(language);

            // assert
            Assert.False(actual.IsValid);
        }

        [Theory]
        [InlineData("Eng")]
        [InlineData("English")]
        [InlineData("Vietnamese")]
        [InlineData("VietnameseEnglishLao")]
        public void Validate_ShouldReturnValid_IfNameValid(string name)
        {
            // arrange
            var language = new Language {Name = name, CreatorId = Guid.NewGuid()};
            // act
            ValidationResult actual = _languageValidator.Validate(language);

            // assert
            Assert.True(actual.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnInvalid_IfGuidIsEmpty()
        {
            // arrange
            var language = new Language {Name = "asdl", CreatorId = Guid.Empty};

            // act
            ValidationResult actual = _languageValidator.Validate(language);

            // assert
            Assert.False(actual.IsValid);
        }
    }
}