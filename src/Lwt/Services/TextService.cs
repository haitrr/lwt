namespace Lwt.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Creators;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Mappers;
    using Lwt.Models;
    using Lwt.Utilities;
    using Lwt.ViewModels;

    /// <summary>
    /// the text service.
    /// </summary>
    public class TextService : ITextService
    {
        private readonly ITextRepository textRepository;

        private readonly ILanguageHelper languageHelper;
        private readonly IMapper<Text, TextViewModel> textViewMapper;
        private readonly ITextCreator textCreator;

        private readonly IMapper<TextEditModel, Text> textEditMapper;
        private readonly IMapper<Text, TextEditDetailModel> textEditDetailMapper;
        private readonly IAsyncMapper<Text, TextReadModel> textReadMapper;
        private readonly ITermCounter termCounter;
        private readonly IUserTextGetter userTextGetter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextService"/> class.
        /// </summary>
        /// <param name="textRepository">textRepository.</param>
        /// <param name="textViewMapper">text view mapper.</param>
        /// <param name="textEditMapper">textEditMapper.</param>
        /// <param name="languageHelper">the language helper.</param>
        /// <param name="textEditDetailMapper">text edit detail mapper.</param>
        /// <param name="textCreator">the text creator.</param>
        /// <param name="termCounter">the term counter.</param>
        /// <param name="userTextGetter">user text getter.</param>
        /// <param name="textReadMapper">text read mapper.</param>
        public TextService(
            ITextRepository textRepository,
            IMapper<TextEditModel, Text> textEditMapper,
            ILanguageHelper languageHelper,
            IMapper<Text, TextViewModel> textViewMapper,
            IMapper<Text, TextEditDetailModel> textEditDetailMapper,
            ITextCreator textCreator,
            ITermCounter termCounter,
            IUserTextGetter userTextGetter,
            IAsyncMapper<Text, TextReadModel> textReadMapper)
        {
            this.textRepository = textRepository;
            this.textEditMapper = textEditMapper;
            this.languageHelper = languageHelper;
            this.textViewMapper = textViewMapper;
            this.textEditDetailMapper = textEditDetailMapper;
            this.textCreator = textCreator;
            this.termCounter = termCounter;
            this.userTextGetter = userTextGetter;
            this.textReadMapper = textReadMapper;
        }

        /// <inheritdoc/>
        public Task CreateAsync(Text text)
        {
            return this.textCreator.CreateAsync(text);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TextViewModel>> GetByUserAsync(
            int userId,
            TextFilter textFilter,
            PaginationQuery paginationQuery)
        {
            IEnumerable<Text> texts = await this.textRepository.GetByUserAsync(userId, textFilter, paginationQuery);
            var viewModels = new List<TextViewModel>();

            foreach (Text text in texts)
            {
                TextViewModel viewModel = this.textViewMapper.Map(text);
                Dictionary<LearningLevel, long> termCountDict =
                    await this.termCounter.CountByLearningLevelAsync(text.Words, text.LanguageCode, text.CreatorId);

                foreach (KeyValuePair<LearningLevel, long> keyValuePair in termCountDict)
                {
                    viewModel.Counts.Add(keyValuePair.Key, keyValuePair.Value);
                }

                viewModels.Add(viewModel);
            }

            return viewModels;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id, int userId)
        {
            Text text = await this.textRepository.GetByIdAsync(id);

            if (text.CreatorId == userId)
            {
                await this.textRepository.DeleteByIdAsync(text);
            }
            else
            {
                throw new ForbiddenException("You don't have permission to delete this text");
            }
        }

        /// <inheritdoc/>
        public async Task EditAsync(int textId, int userId, TextEditModel editModel)
        {
            Text? text = await this.textRepository.TryGetByIdAsync(textId);

            if (text == null)
            {
                throw new NotFoundException("Text not found");
            }

            if (text.CreatorId == userId)
            {
                Text editedText = this.textEditMapper.Map(editModel, text);
                this.SplitText(editedText);
                await this.textRepository.UpdateAsync(editedText);
            }
            else
            {
                throw new ForbiddenException("You do not have permission to edit this text");
            }
        }

        /// <inheritdoc/>
        public Task<long> CountAsync(int userId, TextFilter textFilter)
        {
            return this.textRepository.CountByUserAsync(userId, textFilter);
        }

        /// <inheritdoc />
        public async Task<TextReadModel> ReadAsync(int id, int userId)
        {
            Text text = await this.userTextGetter.GetUserTextAsync(id, userId);

            return await this.textReadMapper.MapAsync(text);
        }

        /// <inheritdoc />
        public async Task<TextEditDetailModel> GetEditDetailAsync(int id, int userId)
        {
            Text? text = await this.textRepository.TryGetByIdAsync(id);

            if (text == null)
            {
                throw new NotFoundException("Text not found.");
            }

            if (text.CreatorId != userId)
            {
                throw new ForbiddenException("You don't have permission to access this text.");
            }

            return this.textEditDetailMapper.Map(text);
        }

        /// <inheritdoc />
        public async Task SetBookmarkAsync(int id, int userId, SetBookmarkModel setBookmarkModel)
        {
            Text text = await this.userTextGetter.GetUserTextAsync(id, userId);

            if (setBookmarkModel.TermIndex >= (ulong)text.Words.Count)
            {
                throw new BadRequestException("Invalid bookmark index.");
            }

            text.Bookmark = setBookmarkModel.TermIndex;

            await this.textRepository.UpdateAsync(text);
        }

        private void SplitText(Text text)
        {
            ILanguage language = this.languageHelper.GetLanguage(text.LanguageCode);
            text.Words = language.SplitText(text.Content);
        }
    }
}