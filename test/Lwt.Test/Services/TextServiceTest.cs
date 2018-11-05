namespace Lwt.Test.Services
{
    using System;
    using System.Threading.Tasks;

    using FluentValidation;
    using FluentValidation.Results;

    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;

    using LWT.Models;

    using Lwt.Services;

    using Moq;

    using Xunit;

    /// <summary>
    /// test text service.
    /// </summary>
    public class TextServiceTest
    {
        private readonly TextService textService;

        private readonly Mock<ITextRepository> textRepository;

        private readonly Mock<ITransaction> transaction;

        private readonly Mock<IMapper<TextEditModel, Text>> textEditMapper;

        private readonly Mock<IValidator<Text>> textValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextServiceTest"/> class.
        /// constructor.
        /// </summary>
        public TextServiceTest()
        {
            this.textEditMapper = new Mock<IMapper<TextEditModel, Text>>();
            this.textRepository = new Mock<ITextRepository>();
            this.transaction = new Mock<ITransaction>();
            this.textValidator = new Mock<IValidator<Text>>();

            this.textService = new TextService(
                this.textRepository.Object,
                this.transaction.Object,
                this.textEditMapper.Object,
                this.textValidator.Object);
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_ShouldThrowException_IfTextNotValid()
        {
            // arrange
            var text = new Text();
            var validationResult = new Mock<ValidationResult>();
            this.textValidator.Setup(v => v.Validate(text)).Returns(validationResult.Object);

            // add an error
            validationResult.Object.Errors.Add(new ValidationFailure("p", "e"));
            validationResult.Setup(r => r.IsValid).Returns(false);

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.textService.CreateAsync(text));
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_ShouldCallRepository_IfTexValid()
        {
            // arrange
            var text = new Text();
            var validationResult = new Mock<ValidationResult>();
            this.textValidator.Setup(v => v.Validate(text)).Returns(validationResult.Object);
            validationResult.Setup(r => r.IsValid).Returns(true);

            // act
            await this.textService.CreateAsync(text);

            // assert
            this.textRepository.Verify(r => r.Add(text), Times.Once);
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetByUserAsync_ShouldCallRepository()
        {
            // arrange
            this.textRepository.Reset();
            Guid userId = Guid.NewGuid();

            // act
            await this.textService.GetByUserAsync(userId);

            // assert
            this.textRepository.Verify(r => r.GetByUserAsync(userId), Times.Once);
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_ShouldThrowException_IfNotCreator()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text { UserId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.DeleteAsync(textId, userId));
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_ShouldCallRepository_IfHasPermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text { UserId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // act
            await this.textService.DeleteAsync(textId, creatorId);

            // assert
            this.textRepository.Verify(r => r.DeleteById(text), Times.Once);
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_ShouldThrowException_IfNotHavePermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var editModel = new TextEditModel();
            var text = new Text { UserId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.EditAsync(textId, userId, editModel));
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EditAsync_ShouldCallRepository_IfHasPermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text { UserId = creatorId };
            var editedText = new Text();
            var editModel = new TextEditModel();
            this.textEditMapper.Setup(m => m.Map(editModel, text)).Returns(editedText);
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // act
            await this.textService.EditAsync(textId, creatorId, editModel);

            // assert
            this.textRepository.Verify(r => r.Update(editedText), Times.Once);
            this.transaction.Verify(t => t.Commit(), Times.Once);
        }
    }
}