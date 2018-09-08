using System;
using System.Threading.Tasks;
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
        public async Task CreateAsync_ShouldReturnTrue_IfSuccess()
        {
            // arrange
            _transaction.Setup(t => t.Commit()).ReturnsAsync(true);

            //act
            bool actual = await _textService.CreateAsync(It.IsAny<Guid>(), new Text());

            //assert
            Assert.True(actual);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallRepository()
        {
            // arrange
            _textRepository.Reset();
            Text text = new Text();

            // act
            await _textService.CreateAsync(Guid.Empty, text);

            //assert
            _textRepository.Verify(r => r.Add(text), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnFalse_IfFail()
        {
            // arrange
            _transaction.Setup(t => t.Commit()).ReturnsAsync(false);

            //act
            bool actual = await _textService.CreateAsync(It.IsAny<Guid>(), new Text());

            //assert
            Assert.False(actual);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldCallRepository()
        {
            // arrange
            _textRepository.Reset();
            var userId = Guid.NewGuid();

            //act
            await _textService.GetByUserAsync(userId);

            // assert
            _textRepository.Verify(r => r.GetByUserAsync(userId), Times.Once);
        }
    }
}