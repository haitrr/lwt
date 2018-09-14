using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lwt.Exceptions;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.Services;
using LWT.Models;
using Moq;
using Xunit;

namespace Lwt.Test.Services
{
    public class TextServiceTest
    {
        private readonly TextService _textService;
        private readonly Mock<ITextRepository> _textRepository;
        private readonly Mock<ITransaction> _transaction;
        private readonly Mock<IMapper<TextEditModel, Text>> _textEditMapper;
        private readonly Mock<IValidator<Text>> _textValidator;

        public TextServiceTest()
        {
            _textEditMapper = new Mock<IMapper<TextEditModel, Text>>();
            _textRepository = new Mock<ITextRepository>();
            _transaction = new Mock<ITransaction>();
            _textValidator = new Mock<IValidator<Text>>();

            _textService = new TextService(_textRepository.Object, _transaction.Object, _textEditMapper.Object,
                _textValidator.Object);
        }


        [Fact]
        public async Task CreateAsync_ShouldThrowException_IfTextNotValid()
        {
            // arrange
            var text = new Text();
            var validationResult = new Mock<ValidationResult>();
            _textValidator.Setup(v => v.Validate(text)).Returns(validationResult.Object);
            // add an error
            validationResult.Object.Errors.Add(new ValidationFailure("p", "e"));
            validationResult.Setup(r => r.IsValid).Returns(false);

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => _textService.CreateAsync(text));
        }

        [Fact]
        public async Task CreateAsync_ShouldCallRepository_IfTexValid()
        {
            // arrange
            var text = new Text();
            var validationResult = new Mock<ValidationResult>();
            _textValidator.Setup(v => v.Validate(text)).Returns(validationResult.Object);
            validationResult.Setup(r => r.IsValid).Returns(true);

            // act
            await _textService.CreateAsync(text);

            //assert
            _textRepository.Verify(r => r.Add(text), Times.Once);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldCallRepository()
        {
            // arrange
            _textRepository.Reset();
            Guid userId = Guid.NewGuid();

            //act
            await _textService.GetByUserAsync(userId);

            // assert
            _textRepository.Verify(r => r.GetByUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_IfNotCreator()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text {UserId = creatorId};
            _textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => _textService.DeleteAsync(textId, userId));
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepository_IfHasPermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text {UserId = creatorId};
            _textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            //act
            await _textService.DeleteAsync(textId, creatorId);

            // assert
            _textRepository.Verify(r => r.DeleteById(text), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_IfNotHavePermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var editModel = new TextEditModel();
            var text = new Text {UserId = creatorId};
            _textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => _textService.EditAsync(textId, userId, editModel));
        }

        [Fact]
        public async Task EditAsync_ShouldCallRepository_IfHasPermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text {UserId = creatorId};
            var editedText = new Text();
            var editModel = new TextEditModel();
            _textEditMapper.Setup(m => m.Map(editModel, text)).Returns(editedText);
            _textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            //act
            await _textService.EditAsync(textId, creatorId, editModel);

            // assert
            _textRepository.Verify(r => r.Update(editedText), Times.Once);
            _transaction.Verify(t => t.Commit(), Times.Once);
        }
    }
}