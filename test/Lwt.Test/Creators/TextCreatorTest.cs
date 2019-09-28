namespace Lwt.Test.Creators
{
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using Lwt.Creators;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Moq;
    using Xunit;

    /// <summary>
    /// test text creator.
    /// </summary>
    public class TextCreatorTest
    {
        private readonly TextCreator textCreator;
        private readonly Mock<IValidator<Text>> textValidatorMock;
        private readonly Mock<ITextSeparator> textSeparatorMock;
        private readonly Mock<ITextRepository> textRepositoryMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextCreatorTest"/> class.
        /// </summary>
        public TextCreatorTest()
        {
            this.textValidatorMock = new Mock<IValidator<Text>>();
            this.textSeparatorMock = new Mock<ITextSeparator>();
            this.textRepositoryMock = new Mock<ITextRepository>();
            this.textCreator = new TextCreator(
                this.textValidatorMock.Object,
                this.textSeparatorMock.Object,
                this.textRepositoryMock.Object);
        }

        /// <summary>
        /// dependency injection should work.
        /// </summary>
        [Fact]
        public void ShouldGetSolved()
        {
            var helper = new DependencyResolverHelper();
            Assert.IsType<TextCreator>(helper.GetService<ITextCreator>());
        }

        /// <summary>
        /// Create method should throw exception if the text
        /// fail validation.
        /// </summary>
        [Fact]
        public void CreateAsyncShouldThrowIfValidationFail()
        {
            var text = new Text();
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure("test", "test"));
            this.textValidatorMock.Setup(m => m.Validate(text)).Returns(validationResult);
            Assert.ThrowsAsync<BadRequestException>(() => this.textCreator.CreateAsync(text));
        }

        /// <summary>
        /// Create method should throw exception if the text
        /// fail validation.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsyncShouldCallRepoToCreateText()
        {
            var text = new Text();
            var validationResult = new ValidationResult();
            this.textValidatorMock.Setup(m => m.Validate(text)).Returns(validationResult);
            await this.textCreator.CreateAsync(text);
            this.textRepositoryMock.Verify(r => r.AddAsync(text), Times.Once);
        }
    }
}