using System;
using System.Linq;
using FluentValidation.Results;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.Validators;
using LWT.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Lwt.Test.Validators
{
    public class TextValidatorTest
    {
        private readonly TextValidator _textValidator;
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<ILanguageRepository> _languageRepository;

        public TextValidatorTest()
        {
            var userStore = new Mock<IUserStore<User>>();

            _userManager =
                new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            _languageRepository = new Mock<ILanguageRepository>();
            _textValidator = new TextValidator(_userManager.Object, _languageRepository.Object);
        }
        
        [Fact]
        public void Validate_ShouldReturnInValid_IfUserIdEmpty()
        {
            // arrange 
            Guid userId = Guid.Empty;
            var text = new Text {UserId = userId};

            // act
            ValidationResult actual = _textValidator.Validate(text);

            //assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.UserId), actual.Errors.First().PropertyName);
        }

        [Fact]
        public void Validate_ShouldReturnInValid_IfLanguageIdEmpty()
        {
            // arrange 
            var text = new Text {UserId = Guid.NewGuid(), Content = "yolo", Title = "yolo", LanguageId = Guid.Empty};
            _languageRepository.Setup(r => r.IsExists(text.LanguageId)).ReturnsAsync(true);
            _userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync(new User());

            // act
            ValidationResult actual = _textValidator.Validate(text);

            //assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.LanguageId), actual.Errors.Single().PropertyName);
        }

        [Fact]
        public void Validate_ShouldReturnInValid_IfTitleEmpty()
        {
            // arrange 
            var text = new Text
                {UserId = Guid.NewGuid(), Content = "valid", Title = "", LanguageId = Guid.NewGuid()};
            _languageRepository.Setup(r => r.IsExists(text.LanguageId)).ReturnsAsync(true);
            _userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync(new User());

            // act
            ValidationResult actual = _textValidator.Validate(text);

            //assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.Title), actual.Errors.Single().PropertyName);
        }

        [Fact]
        public void Validate_ShouldReturnInValid_IfContentEmpty()
        {
            // arrange 
            var text = new Text
                {UserId = Guid.NewGuid(), Content = "", Title = "yolo", LanguageId = Guid.NewGuid()};
            _languageRepository.Setup(r => r.IsExists(text.LanguageId)).ReturnsAsync(true);
            _userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync(new User());

            // act
            ValidationResult actual = _textValidator.Validate(text);

            //assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.Content), actual.Errors.Single().PropertyName);
        }

        [Fact]
        public void Validate_ShouldReturnInValid_IfUserNotExist()
        {
            // arrange 
            var text = new Text
                {UserId = Guid.NewGuid(), Content = "yolo", Title = "yolo", LanguageId = Guid.NewGuid()};
            _languageRepository.Setup(r => r.IsExists(text.LanguageId)).ReturnsAsync(true);
            _userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync((User) null);

            // act
            ValidationResult actual = _textValidator.Validate(text);

            //assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.UserId), actual.Errors.Single().PropertyName);
        }

        [Fact]
        public void Validate_ShouldReturnInValid_IfLanguageNotExist()
        {
            // arrange 
            Guid languageId = Guid.NewGuid();
            var text = new Text {UserId = Guid.NewGuid(), Content = "yolo", Title = "yolo", LanguageId = languageId};
            _userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync(new User());
            _languageRepository.Setup(r => r.IsExists(languageId)).ReturnsAsync(false);

            // act
            ValidationResult actual = _textValidator.Validate(text);

            //assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.LanguageId), actual.Errors.Single().PropertyName);
        }
    }
}