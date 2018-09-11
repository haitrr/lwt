using System;
using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Interfaces;
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

        public TextServiceTest()
        {
            _textRepository = new Mock<ITextRepository>();
            _transaction = new Mock<ITransaction>();
            _textService = new TextService(_textRepository.Object, _transaction.Object);
        }

        [Fact]
        public void CreateAsync_ShouldWork_IfSuccess()
        {
            // arrange
            Task expect = Task.CompletedTask;
            _transaction.Setup(t => t.Commit()).Returns(expect);

            //act
            Task actual = _textService.CreateAsync(Guid.NewGuid(), new Text());

            //assert
            Assert.Equal(expect, actual);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallRepository()
        {
            // arrange
            _textRepository.Reset();
            var text = new Text();

            // act
            await _textService.CreateAsync(Guid.Empty, text);

            //assert
            _textRepository.Verify(r => r.Add(text), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldWork()
        {
            // arrange
            //act
            //assert
            await _textService.CreateAsync(It.IsAny<Guid>(), new Text());
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
    }
}