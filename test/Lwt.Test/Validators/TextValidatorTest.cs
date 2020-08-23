namespace Lwt.Test.Validators
{
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
        public void ValidateShouldReturnInValidIfUserIdEmpty()
        {
            // arrange
            var userId = 1;
            var text = new Text { CreatorId = userId };

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(
                nameof(text.CreatorId),
                actual.Errors.First().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void ValidateShouldReturnInValidIfTitleEmpty()
        {
            // arrange
            var text = new Text { CreatorId = 1, Content = "valid", Title = string.Empty };

            this.userManager.Setup(m => m.FindByIdAsync(text.CreatorId.ToString()))
                .ReturnsAsync(new User());

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(
                nameof(text.Title),
                actual.Errors.Single().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void ValidateShouldReturnInValidIfContentEmpty()
        {
            // arrange
            var text = new Text { CreatorId = 1, Content = string.Empty, Title = "yolo" };

            this.userManager.Setup(m => m.FindByIdAsync(text.CreatorId.ToString()))
                .ReturnsAsync(new User());

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(
                nameof(text.Content),
                actual.Errors.Single().PropertyName);
        }

        /// <summary>
        /// a.
        /// </summary>
        [Fact]
        public void ValidateShouldReturnInValidIfUserNotExist()
        {
            // arrange
            var text = new Text { CreatorId = 1, Content = "yolo", Title = "yolo" };
#nullable disable
            this.userManager.Setup(m => m.FindByIdAsync(text.CreatorId.ToString()))
                .ReturnsAsync((User)null);
#nullable disable

            // act
            ValidationResult actual = this.textValidator.Validate(text);

            // assert
            Assert.False(actual.IsValid);
            Assert.Equal(
                nameof(text.CreatorId),
                actual.Errors.Single().PropertyName);
        }
    }
}