namespace Lwt.Test.Validators
{
    using System;
    using System.Linq;

    using FluentValidation.Results;

    using Lwt.Models;
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

            this.textValidator = new TextValidator(this.userManager.Object);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInValid_IfUserIdEmpty()
        {
            // arrange
            Guid userId = Guid.Empty;
            var text = new Text { CreatorId = userId };

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.CreatorId), actual.Errors.First().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnInValid_IfTitleEmpty()
        {
            // arrange
            var text = new Text { CreatorId = Guid.NewGuid(), Content = "valid", Title = string.Empty };

            this.userManager.Setup(m => m.FindByIdAsync(text.CreatorId.ToString())).ReturnsAsync(new User());

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
            var text = new Text { CreatorId = Guid.NewGuid(), Content = string.Empty, Title = "yolo" };

            this.userManager.Setup(m => m.FindByIdAsync(text.CreatorId.ToString())).ReturnsAsync(new User());

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
            var text = new Text { CreatorId = Guid.NewGuid(), Content = "yolo", Title = "yolo" };

            this.userManager.Setup(m => m.FindByIdAsync(text.CreatorId.ToString())).ReturnsAsync((User)null);

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(nameof(text.CreatorId), actual.Errors.Single().PropertyName);
        }
    }
}