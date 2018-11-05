namespace Lwt.Test.Validators
{
    using System;
    using System.Linq;

    using FluentValidation.Results;

    using Lwt.Interfaces;
    using Lwt.Models;

    using LWT.Models;

    using Lwt.Validators;

    using Microsoft.AspNetCore.Identity;

    using Moq;

    using Xunit;

    /// <summary>
    /// a.
    /// </summary>
    public class TextValidatorTest
    {
        private readonly TextValidator textValidator;

        private readonly Mock<UserManager<User>> userManager;

        private readonly Mock<ILanguageRepository> languageRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextValidatorTest"/> class.
        /// a.
        /// </summary>
        public TextValidatorTest()
        {
            var userStore = new Mock<IUserStore<User>>();

            this.userManager = new Mock<UserManager<User>>(
                userStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            this.languageRepository = new Mock<ILanguageRepository>();
            this.textValidator = new TextValidator(this.userManager.Object, this.languageRepository.Object);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInValid_IfUserIdEmpty()
        {
            // arrange
            Guid userId = Guid.Empty;
            var text = new Text { UserId = userId };

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.UserId), actual.Errors.First().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInValid_IfLanguageIdEmpty()
        {
            // arrange
            var text = new Text { UserId = Guid.NewGuid(), Content = "yolo", Title = "yolo", LanguageId = Guid.Empty };
            this.languageRepository.Setup(r => r.IsExists(text.LanguageId)).ReturnsAsync(true);
            this.userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync(new User());

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.LanguageId), actual.Errors.Single().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInValid_IfTitleEmpty()
        {
            // arrange
            var text = new Text
                { UserId = Guid.NewGuid(), Content = "valid", Title = string.Empty, LanguageId = Guid.NewGuid() };

            this.languageRepository.Setup(r => r.IsExists(text.LanguageId)).ReturnsAsync(true);
            this.userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync(new User());

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.Title), actual.Errors.Single().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInValid_IfContentEmpty()
        {
            // arrange
            var text = new Text
                { UserId = Guid.NewGuid(), Content = string.Empty, Title = "yolo", LanguageId = Guid.NewGuid() };

            this.languageRepository.Setup(r => r.IsExists(text.LanguageId)).ReturnsAsync(true);
            this.userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync(new User());

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.Content), actual.Errors.Single().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInValid_IfUserNotExist()
        {
            // arrange
            var text = new Text
                { UserId = Guid.NewGuid(), Content = "yolo", Title = "yolo", LanguageId = Guid.NewGuid() };

            this.languageRepository.Setup(r => r.IsExists(text.LanguageId)).ReturnsAsync(true);
            this.userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync((User)null);

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.UserId), actual.Errors.Single().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInValid_IfLanguageNotExist()
        {
            // arrange
            Guid languageId = Guid.NewGuid();
            var text = new Text { UserId = Guid.NewGuid(), Content = "yolo", Title = "yolo", LanguageId = languageId };
            this.userManager.Setup(m => m.FindByIdAsync(text.UserId.ToString())).ReturnsAsync(new User());
            this.languageRepository.Setup(r => r.IsExists(languageId)).ReturnsAsync(false);

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.LanguageId), actual.Errors.Single().PropertyName);
        }
    }
}