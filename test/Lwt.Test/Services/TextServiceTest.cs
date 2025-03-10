namespace Lwt.Test.Services;

/// <summary>
/// test text service.
/// </summary>
public class TextServiceTest
{
    // private readonly TextService textService;
    //
    //     private readonly Mock<ISqlTextRepository> textRepository;
    //
    //     private readonly Mock<ILanguageHelper> languageHelper;
    //
    //     private readonly Mock<IMapper<TextEditModel, Text>> textEditMapper;
    //     private readonly Mock<IMapper<Text, TextViewModel>> textViewMapper;
    //     private readonly Mock<IMapper<Text, TextEditDetailModel>> textEditDetailMapper;
    //     private readonly Mock<IAsyncMapper<Text, TextReadModel>> textReadMapper;
    //
    //     private readonly Mock<ITextCreator> textCreator;
    //
    //     private readonly Mock<IUserTextGetter> userTextGetterMock;
    //     private readonly Mock<ITermCounter> termCounterMock;
    //     private readonly Mock<ITextTermRepository> textTermRepositoryMock;
    //     private readonly Mock<IDbTransaction> dbTransactionMock;
    //
    //     /// <summary>
    //     /// Initializes a new instance of the <see cref="TextServiceTest"/> class.
    //     /// constructor.
    //     /// </summary>
    //     public TextServiceTest()
    //     {
    //         this.textEditMapper = new Mock<IMapper<TextEditModel, Text>>();
    //         this.textViewMapper = new Mock<IMapper<Text, TextViewModel>>();
    //         this.textEditDetailMapper = new Mock<IMapper<Text, TextEditDetailModel>>();
    //         this.textRepository = new Mock<ISqlTextRepository>();
    //         this.languageHelper = new Mock<ILanguageHelper>();
    //         this.textCreator = new Mock<ITextCreator>();
    //         this.termCounterMock = new Mock<ITermCounter>();
    //         this.userTextGetterMock = new Mock<IUserTextGetter>();
    //         this.dbTransactionMock = new Mock<IDbTransaction>();
    //         this.textTermRepositoryMock = new Mock<ITextTermRepository>();
    //         this.textReadMapper = new Mock<IAsyncMapper<Text, TextReadModel>>();
    //
    //         this.textService = new TextService(
    //             this.textRepository.Object,
    //             this.textEditMapper.Object,
    //             this.languageHelper.Object,
    //             this.textViewMapper.Object,
    //             this.textEditDetailMapper.Object,
    //             this.textCreator.Object,
    //             this.termCounterMock.Object,
    //             this.userTextGetterMock.Object,
    //             this.textReadMapper.Object,
    //             this.dbTransactionMock.Object,
    //             this.textTermRepositoryMock.Object);
    //     }
    //
    //     /// <summary>
    //     /// dependency injection should work.
    //     /// </summary>
    //     [Fact]
    //     public void ShouldGetSolved()
    //     {
    //         var helper = new DependencyResolverHelper();
    //         helper.GetService<ITextService>();
    //     }
    //
    //     /// <summary>
    //     /// Delete should throw forbidden exception
    //     /// if the user calling is not owner of the text.
    //     /// </summary>
    //     [Fact]
    //     public void DeleteAsyncShouldThrowExceptionIfNotOwner()
    //     {
    //         var textId = 1;
    //         var userId = 1;
    //         var text = new Text { CreatorId = 1 };
    //         this.textRepository.Setup(r => r.TryGetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //         Assert.ThrowsAsync<ForbiddenException>(() => this.textService.DeleteAsync(textId, userId));
    //     }
    //
    //     /// <summary>
    //     /// Delete should call repository to delete the text.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public async Task DeleteAsyncShouldCallRepository()
    //     {
    //         var textId = 1;
    //         var userId = 1;
    //         var text = new Text { CreatorId = userId };
    //         this.textRepository.Reset();
    //         this.textRepository.Setup(r => r.GetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //
    //         await this.textService.DeleteAsync(textId, userId);
    //
    //         this.textRepository.Verify(r => r.Delete(text), Times.Once);
    //     }
    //
    //     /// <summary>
    //     /// test.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public async Task DeleteAsyncShouldThrowExceptionIfNotCreator()
    //     {
    //         // arrange
    //         var creatorId = 1;
    //         var userId = 1;
    //         var textId = 1;
    //         var text = new Text { CreatorId = creatorId };
    //         this.textRepository.Setup(r => r.GetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //
    //         // assert
    //         await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.DeleteAsync(textId, userId));
    //     }
    //
    //     /// <summary>
    //     /// test.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public async Task DeleteAsyncShouldCallRepositoryIfHasPermission()
    //     {
    //         // arrange
    //         var creatorId = 1;
    //         var textId = 1;
    //         var text = new Text { CreatorId = creatorId };
    //         this.textRepository.Setup(r => r.GetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //
    //         // act
    //         await this.textService.DeleteAsync(textId, creatorId);
    //
    //         // assert
    //         this.textRepository.Verify(r => r.Delete(text), Times.Once);
    //     }
    //
    //     /// <summary>
    //     /// test.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public async Task EditAsyncShouldThrowExceptionIfNotHavePermission()
    //     {
    //         // arrange
    //         var creatorId = 1;
    //         var userId = 1;
    //         var textId = 1;
    //         var editModel = new TextEditModel();
    //         var text = new Text { CreatorId = creatorId };
    //         this.textRepository.Setup(r => r.TryGetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //
    //         // assert
    //         await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.EditAsync(textId, userId, editModel));
    //     }
    //
    //     /// <summary>
    //     /// should throw not found exception if text not found.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public async Task EditAsyncShouldThrowNotFoundExceptionIfTextNotFound()
    //     {
    //         // arrange
    //         var userId = 1;
    //         var textId = 1;
    //         var editModel = new TextEditModel();
    //         this.textRepository.Setup(r => r.TryGetByIdAsync(textId))
    //             .ReturnsAsync((Text?)null);
    //
    //         // assert
    //         await Assert.ThrowsAsync<NotFoundException>(() => this.textService.EditAsync(textId, userId, editModel));
    //     }
    //
    //     /// <summary>
    //     /// should throw forbidden exception if user edit text is not the creator of the text.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public async Task EditAsyncShouldThrowForbiddenExceptionIfNotCreator()
    //     {
    //         // arrange
    //         var userId = 1;
    //         var textId = 1;
    //         var editModel = new TextEditModel();
    //         var text = new Text { CreatorId = 1 };
    //         this.textRepository.Setup(r => r.TryGetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //
    //         // assert
    //         await Assert.ThrowsAsync<ForbiddenException>(() => this.textService.EditAsync(textId, userId, editModel));
    //     }
    //
    //     /// <summary>
    //     /// the service should call the repository for update the text.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public async Task EditAsyncShouldWorkIfHappyCase()
    //     {
    //         var userId = 1;
    //         var textId = 1;
    //         var editModel = new TextEditModel();
    //         var text = new Text { CreatorId = userId };
    //         var editedText = new Text();
    //         var language = new Mock<ILanguage>();
    //         this.textRepository.Setup(r => r.TryGetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //         this.textEditMapper.Setup(t => t.Map(editModel, text))
    //             .Returns(editedText);
    //         this.languageHelper.Setup(t => t.GetLanguage(text.LanguageCode))
    //             .Returns(language.Object);
    //         await this.textService.EditAsync(textId, userId, editModel);
    //         this.textRepository.Verify(r => r.Update(editedText), Times.Once);
    //     }
    //
    //     /// <summary>
    //     /// get by user async should work.
    //     /// </summary>
    //     /// <param name="textNumber">number of text.</param>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [InlineData(1)]
    //     [InlineData(3)]
    //     [InlineData(2)]
    //     [Theory]
    //     public async Task GetByUserAsyncShouldWork(int textNumber)
    //     {
    //         var userId = 1;
    //         var textFilter = new TextFilter();
    //         var paginationQuery = new PaginationQuery();
    //         var texts = new List<Text>();
    //         var countDict = new Dictionary<LearningLevel, long>
    //         {
    //             { LearningLevel.Learning1, 321 }, { LearningLevel.Unknown, 21 },
    //         };
    //
    //         for (var i = 0; i < textNumber; i++)
    //         {
    //             texts.Add(new Text());
    //         }
    //
    //         this.textViewMapper.Setup(m => m.Map(It.IsAny<Text>()))
    //             .Returns(() => new TextViewModel());
    //         this.textRepository.Setup(r => r.GetByUserAsync(userId, textFilter, paginationQuery))
    //             .ReturnsAsync(texts);
    //         this.termCounterMock.Setup(
    //                 c => c.CountByLearningLevelAsync(
    //                     It.IsAny<IEnumerable<string>>(),
    //                     It.IsAny<LanguageCode>(),
    //                     It.IsAny<int>()))
    //             .ReturnsAsync(countDict);
    //
    //         IEnumerable<TextViewModel> viewModels =
    //             await this.textService.GetByUserAsync(userId, textFilter, paginationQuery);
    //         Assert.Equal(viewModels.Count(), texts.Count());
    //     }
    //
    //     /// <summary>
    //     /// should throw not found exception if text not found.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public Task GetEditDetailShouldThrowNotFoundIfTextNotFound()
    //     {
    //         var userId = 1;
    //         var textId = 1;
    //         this.textRepository.Setup(r => r.TryGetByIdAsync(textId))
    //             .ReturnsAsync((Text?)null);
    //         return Assert.ThrowsAsync<NotFoundException>(() => this.textService.GetEditDetailAsync(textId, userId));
    //     }
    //
    //     /// <summary>
    //     /// should throw forbidden exception if user is not owner of the text.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public Task GetEditDetailShouldThrowForbiddenIfUserIsNotOwner()
    //     {
    //         var userId = 1;
    //         var textId = 1;
    //         var onwerId = 1;
    //         var text = new Text { CreatorId = onwerId };
    //         this.textRepository.Setup(r => r.TryGetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //         return Assert.ThrowsAsync<ForbiddenException>(() => this.textService.GetEditDetailAsync(textId, userId));
    //     }
    //
    //     /// <summary>
    //     /// get edit detail should work in happy case.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    //     [Fact]
    //     public async Task GetEditDetailShouldWork()
    //     {
    //         var userId = 1;
    //         var textId = 1;
    //         var detailModel = new TextEditDetailModel();
    //         var text = new Text { CreatorId = userId };
    //         this.textRepository.Setup(r => r.TryGetByIdAsync(textId))
    //             .ReturnsAsync(text);
    //         this.textEditDetailMapper.Setup(r => r.Map(text))
    //             .Returns(detailModel);
    //
    //         TextEditDetailModel result = await this.textService.GetEditDetailAsync(textId, userId);
    //         Assert.Equal(detailModel, result);
    //     }
    //
    //     /// <summary>
    //     /// set bookmark async should throw bad request exception if term index is not valid.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    //     [Fact]
    //     public async Task SetBookmarkAsyncShouldThrowBadRequestIfTermIndexIsInvalid()
    //     {
    //         ulong termIndex = 2;
    //         var setBookmarkModel = new SetBookmarkModel { TermIndex = termIndex };
    //         var textId = 1;
    //         var userId = 1;
    //         var text = new Text();
    //         // text.Words = new List<string> { "hello", "unitest" };
    //         this.userTextGetterMock.Setup(r => r.GetUserTextAsync(textId, userId))
    //             .ReturnsAsync(text);
    //
    //         await Assert.ThrowsAsync<BadRequestException>(
    //             () => this.textService.SetBookmarkAsync(textId, userId, setBookmarkModel));
    //     }
    //
    //     /// <summary>
    //     /// should set text bookmark.
    //     /// </summary>
    //     /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    //     [Fact]
    //     public async Task SetBookmarkAsyncShouldUpdateTheText()
    //     {
    //         ulong termIndex = 2;
    //         var setBookmarkModel = new SetBookmarkModel { TermIndex = termIndex };
    //         var textId = 1;
    //         var userId = 1;
    //         var text = new Text();
    //         // text.Words = new List<string> { "hello", "vscode", "unitest" };
    //         this.userTextGetterMock.Setup(r => r.GetUserTextAsync(textId, userId))
    //             .ReturnsAsync(text);
    //
    //         await this.textService.SetBookmarkAsync(textId, userId, setBookmarkModel);
    //
    //         Assert.Equal(termIndex, text.Bookmark);
    //         this.textRepository.Verify(r => r.Update(text), Times.Once);
    //     }
}