namespace Lwt.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Creators;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;
    using Lwt.Repositories;
    using Lwt.Utilities;
    using Lwt.ViewModels;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// the text service.
    /// </summary>
    public class TextService : ITextService
    {
        private readonly ISqlTextRepository textRepository;

        private readonly IMapper<Text, TextViewModel> textViewMapper;
        private readonly ITextCreator textCreator;

        private readonly IMapper<TextEditModel, Text> textEditMapper;
        private readonly IMapper<Text, TextEditDetailModel> textEditDetailMapper;
        private readonly IMapper<Text, TextReadModel> textReadMapper;
        private readonly IDbTransaction dbTransaction;
        private readonly ITextTermRepository textTermRepository;
        private readonly IUserTextGetter userTextGetter;
        private readonly ITextTermProcessor textTermProcessor;
        private readonly IMapper<TextTerm, TermReadModel> textTermMapper;

        public TextService(
            ISqlTextRepository textRepository,
            IMapper<TextEditModel, Text> textEditMapper,
            IMapper<Text, TextViewModel> textViewMapper,
            IMapper<Text, TextEditDetailModel> textEditDetailMapper,
            ITextCreator textCreator,
            IUserTextGetter userTextGetter,
            IMapper<Text, TextReadModel> textReadMapper,
            IDbTransaction dbTransaction,
            ITextTermRepository textTermRepository,
            ITextTermProcessor termProcessor,
            IMapper<TextTerm, TermReadModel> textTermMapper)
        {
            this.textRepository = textRepository;
            this.textEditMapper = textEditMapper;
            this.textViewMapper = textViewMapper;
            this.textEditDetailMapper = textEditDetailMapper;
            this.textCreator = textCreator;
            this.userTextGetter = userTextGetter;
            this.textReadMapper = textReadMapper;
            this.dbTransaction = dbTransaction;
            this.textTermRepository = textTermRepository;
            this.textTermProcessor = termProcessor;
            this.textTermMapper = textTermMapper;
        }

        /// <inheritdoc/>
        public Task<int> CreateAsync(Text text)
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
                viewModels.Add(viewModel);
            }

            return viewModels;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id, int userId)
        {
            Text text = await this.textRepository.GetByIdAsync(id);

            if (text.UserId == userId)
            {
                this.textRepository.Delete(text);
                await this.dbTransaction.CommitAsync();
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

            if (text.UserId == userId)
            {
                Text editedText = this.textEditMapper.Map(editModel, text);
                this.textRepository.Update(editedText);
                await this.textTermProcessor.ProcessTextTermAsync(editedText);
                await this.dbTransaction.CommitAsync();
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
            Text? text = await this.textRepository.Queryable()
                .Where(t => t.Id == id && t.UserId == userId)
                .Select(
                    t => new Text() {
                        Id = t.Id,
                        Title = t.Title,
                        Bookmark = t.Bookmark,
                        LanguageCode = t.LanguageCode
                    })
                .SingleOrDefaultAsync();

            if (text == null)
            {
                throw new NotFoundException("Text not found.");
            }

            return this.textReadMapper.Map(text);
        }

        /// <inheritdoc />
        public async Task<TextEditDetailModel> GetEditDetailAsync(int id, int userId)
        {
            Text? text = await this.textRepository.TryGetByIdAsync(id);

            if (text == null)
            {
                throw new NotFoundException("Text not found.");
            }

            if (text.UserId != userId)
            {
                throw new ForbiddenException("You don't have permission to access this text.");
            }

            return this.textEditDetailMapper.Map(text);
        }

        /// <inheritdoc />
        public async Task SetBookmarkAsync(int id, int userId, SetBookmarkModel setBookmarkModel)
        {
            Text text = await this.userTextGetter.GetUserTextAsync(id, userId);

            int textTermCount = await this.textTermRepository.CountByTextAsync(text.Id);

            if (setBookmarkModel.TermIndex >= (ulong)textTermCount)
            {
                throw new BadRequestException("Invalid bookmark index.");
            }

            text.Bookmark = setBookmarkModel.TermIndex;

            this.textRepository.Update(text);
            await this.dbTransaction.CommitAsync();
        }

        public async Task<IDictionary<LearningLevel, int>> GetTermCountsAsync(int id, int userId)
        {
            Text text = await this.userTextGetter.GetUserTextAsync(id, userId);
            return await this.textTermRepository.CountTextTermByLearningLevelAsync(text.Id);
        }

        public async Task<int> CountTextTermsAsync(int id, int userId)
        {
            Text text = await this.userTextGetter.GetUserTextAsync(id, userId);
            return await this.textTermRepository.CountByTextAsync(text.Id);
        }

        public async Task<IEnumerable<TermReadModel>> GetTextTermsAsync(int id, int userId, int indexFrom, int indexTo)
        {
            Text text = await this.userTextGetter.GetUserTextAsync(id, userId);
            IEnumerable<TextTerm> textTerms = await this.textTermRepository.GetByTextAsync(text.Id, indexFrom, indexTo);
            return this.textTermMapper.Map(textTerms);
        }

        public async Task<int> GetTermCountInTextAsync(int id, int userId, int termId)
        {
            Text text = await this.userTextGetter.GetUserTextAsync(id, userId);
            int count = await this.textTermRepository.GetTermCountInTextAsync(text.Id, termId);
            return count;
        }
    }
}