namespace Lwt.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Creators;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;
    using Lwt.Services;
    using Lwt.ViewModels;
    using Moq;
    using Xunit;

    /// <summary>
    /// test text service.
    /// </summary>
    public class TextServiceTest
    {
        private readonly TextService textService;

        private readonly Mock<ITextRepository> textRepository;

        private readonly Mock<ILanguageHelper> languageHelper;

        private readonly Mock<IMapper<TextEditModel, Text>> textEditMapper;
        private readonly Mock<IMapper<Text, TextViewModel>> textViewMapper;
        private readonly Mock<IMapper<Text, TextEditDetailModel>> textEditDetailMapper;

        private readonly Mock<ITextCreator> textCreator;

        private readonly Mock<ITermRepository> termRepository;
        private readonly Mock<ITermCounter> termCounterMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextServiceTest"/> class.
        /// constructor.
        /// </summary>
        public TextServiceTest()
        {
            this.textEditMapper = new Mock<IMapper<TextEditModel, Text>>();
            this.textViewMapper = new Mock<IMapper<Text, TextViewModel>>();
            this.textEditDetailMapper = new Mock<IMapper<Text, TextEditDetailModel>>();
            this.textRepository = new Mock<ITextRepository>();
            this.languageHelper = new Mock<ILanguageHelper>();
            this.termRepository = new Mock<ITermRepository>();
            this.textCreator = new Mock<ITextCreator>();
            this.termCounterMock = new Mock<ITermCounter>();

            this.textService = new TextService(
                this.textRepository.Object,
                this.textEditMapper.Object,
                this.languageHelper.Object,
                this.termRepository.Object,
                this.textViewMapper.Object,
                this.textEditDetailMapper.Object,
                this.textCreator.Object,
                this.termCounterMock.Object);
        }

        /// <summary>
        /// dependency injection should work.
        /// </summary>
        [Fact]
        public void ShouldGetSolved()
        {
            var helper = new DependencyResolverHelper();
            helper.GetService<ITextService>();
        }

        /// <summary>
        /// Delete should throw forbidden exception
        /// if the user calling is not owner of the text.
        /// </summary>
        [Fact]
        public void DeleteAsyncShouldThrowExceptionIfNotOwner()
        {
            Guid textId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var text = new Text { CreatorId = Guid.NewGuid() };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);
            Assert.ThrowsAsync<ForbiddenException>(() => this.textService.DeleteAsync(textId, userId));
        }

        /// <summary>
        /// Delete should call repository to delete the text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsyncShouldCallRepository()
        {
             Guid textId = Guid.NewGuid();
             Guid userId = Guid.NewGuid();
             var text = new Text { CreatorId = userId };
             this.textRepository.Reset();
             this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

             await this.textService.DeleteAsync(textId, userId);

             this.textRepository.Verify(
                 r => r.DeleteByIdAsync(text),
                 Times.Once);
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsyncShouldThrowExceptionIfNotCreator()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text { CreatorId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.DeleteAsync(textId, userId));
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsyncShouldCallRepositoryIfHasPermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var text = new Text { CreatorId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // act
            await this.textService.DeleteAsync(textId, creatorId);

            // assert
            this.textRepository.Verify(r => r.DeleteByIdAsync(text), Times.Once);
        }

        /// <summary>
        /// test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EditAsyncShouldThrowExceptionIfNotHavePermission()
        {
            // arrange
            Guid creatorId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var editModel = new TextEditModel();
            var text = new Text { CreatorId = creatorId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.EditAsync(textId, userId, editModel));
        }

        /// <summary>
        /// should throw not found exception if text not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EditAsyncShouldThrowNotFoundExceptionIfTextNotFound()
        {
            // arrange
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var editModel = new TextEditModel();
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync((Text)null);

            // assert
            await Assert.ThrowsAsync<NotFoundException>(() => this.textService.EditAsync(textId, userId, editModel));
        }

        /// <summary>
        /// should throw forbidden exception if user edit text is not the creator of the text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EditAsyncShouldThrowForbiddenExceptionIfNotCreator()
        {
            // arrange
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var editModel = new TextEditModel();
            var text = new Text { CreatorId = Guid.NewGuid() };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);

            // assert
            await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.EditAsync(textId, userId, editModel));
        }

        /// <summary>
        /// the service should call the repository for update the text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EditAsyncShouldWorkIfHappyCase()
        {
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var editModel = new TextEditModel();
            var text = new Text { CreatorId = userId };
            var editedText = new Text();
            var language = new Mock<ILanguage>();
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);
            this.textEditMapper.Setup(t => t.Map(editModel, text)).Returns(editedText);
            this.languageHelper.Setup(t => t.GetLanguage(text.Language)).Returns(language.Object);
            await this.textService.EditAsync(textId, userId, editModel);
            this.textRepository.Verify(r => r.UpdateAsync(editedText), Times.Once);
        }

        /// <summary>
        /// get by user async should work.
        /// </summary>
        /// <param name="textNumber">number of text.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(2)]
        [Theory]
        public async Task GetByUserAsyncShouldWork(int textNumber)
        {
            Guid userId = Guid.NewGuid();
            var textFilter = new TextFilter();
            var paginationQuery = new PaginationQuery();
            var texts = new List<Text>();
            var countDict = new Dictionary<TermLearningLevel, long>
            {
                { TermLearningLevel.Learning1, 321 },
                { TermLearningLevel.UnKnow, 21 },
            };

            for (var i = 0; i < textNumber; i++)
            {
                texts.Add(new Text());
            }

            this.textViewMapper.Setup(m => m.Map(It.IsAny<Text>())).Returns(() => new TextViewModel());
            this.textRepository.Setup(r => r.GetByUserAsync(userId, textFilter, paginationQuery)).ReturnsAsync(texts);
            this.termCounterMock.Setup(
                    c => c.CountByLearningLevelAsync(
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<Language>(),
                        It.IsAny<Guid>()))
                .ReturnsAsync(countDict);

            IEnumerable<TextViewModel> viewModels = await this.textService.GetByUserAsync(userId, textFilter, paginationQuery);
            Assert.Equal(viewModels.Count(), texts.Count());
        }

        /// <summary>
        /// should throw not found exception if text not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public Task GetEditDetailShouldThrowNotFoundIfTextNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync((Text)null);
            return Assert.ThrowsAsync<NotFoundException>(() => this.textService.GetEditDetailAsync(textId, userId));
        }

        /// <summary>
        /// should throw forbidden exception if user is not owner of the text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public Task GetEditDetailShouldThrowForbiddenIfUserIsNotOwner()
        {
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            Guid onwerId = Guid.NewGuid();
            var text = new Text { CreatorId = onwerId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);
            return Assert.ThrowsAsync<ForbiddenException>(() => this.textService.GetEditDetailAsync(textId, userId));
        }

        /// <summary>
        /// get edit detail should work in happy case.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetEditDetailShouldWork()
        {
            Guid userId = Guid.NewGuid();
            Guid textId = Guid.NewGuid();
            var detailModel = new TextEditDetailModel();
            var text = new Text { CreatorId = userId };
            this.textRepository.Setup(r => r.GetByIdAsync(textId)).ReturnsAsync(text);
            this.textEditDetailMapper.Setup(r => r.Map(text)).Returns(detailModel);

            TextEditDetailModel result = await this.textService.GetEditDetailAsync(textId, userId);
            Assert.Equal(detailModel, result);
        }
    }
}