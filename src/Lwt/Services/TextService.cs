namespace Lwt.Services
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
        private readonly ITermRepository termRepository;

        private readonly IMapper<TextEditModel, Text> textEditMapper;
        private readonly IMapper<Text, TextEditDetailModel> textEditDetailMapper;
        private readonly ITermCounter termCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextService"/> class.
        /// </summary>
        /// <param name="textRepository">textRepository.</param>
        /// <param name="textViewMapper">text view mapper.</param>
        /// <param name="textEditMapper">textEditMapper.</param>
        /// <param name="languageHelper">the language helper.</param>
        /// <param name="termRepository">the term repository.</param>
        /// <param name="textEditDetailMapper">text edit detail mapper.</param>
        /// <param name="textCreator">the text creator.</param>
        /// <param name="termCounter">the term counter.</param>
        public TextService(
            ITextRepository textRepository,
            IMapper<TextEditModel, Text> textEditMapper,
            ILanguageHelper languageHelper,
            ITermRepository termRepository,
            IMapper<Text, TextViewModel> textViewMapper,
            IMapper<Text, TextEditDetailModel> textEditDetailMapper,
            ITextCreator textCreator,
            ITermCounter termCounter)
        {
            this.textRepository = textRepository;
            this.textEditMapper = textEditMapper;
            this.languageHelper = languageHelper;
            this.termRepository = termRepository;
            this.textViewMapper = textViewMapper;
            this.textEditDetailMapper = textEditDetailMapper;
            this.textCreator = textCreator;
            this.termCounter = termCounter;
        }

        /// <inheritdoc/>
        public Task CreateAsync(Text text)
        {
            return this.textCreator.CreateAsync(text);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TextViewModel>> GetByUserAsync(
            Guid userId,
            TextFilter textFilter,
            PaginationQuery paginationQuery)
        {
            IEnumerable<Text> texts = await this.textRepository.GetByUserAsync(userId, textFilter, paginationQuery);
            var viewModels = new List<TextViewModel>();

            foreach (Text text in texts)
            {
                TextViewModel viewModel = this.textViewMapper.Map(text);
                Dictionary<TermLearningLevel, long> termCountDict =
                    await this.termCounter.CountByLearningLevelAsync(text.Words, text.Language, text.CreatorId);

                foreach (KeyValuePair<TermLearningLevel, long> keyValuePair in termCountDict)
                {
                    viewModel.Counts.Add(keyValuePair.Key, keyValuePair.Value);
                }

                viewModels.Add(viewModel);
            }

            return viewModels;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id, Guid userId)
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
        public async Task EditAsync(Guid textId, Guid userId, TextEditModel editModel)
        {
            Text text = await this.textRepository.GetByIdAsync(textId);

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
        public Task<long> CountAsync(Guid userId, TextFilter textFilter)
        {
            return this.textRepository.CountByUserAsync(userId, textFilter);
        }

        /// <inheritdoc />
        public async Task<TextReadModel> ReadAsync(Guid id, Guid userId)
        {
            Text text = await this.textRepository.GetByIdAsync(id);

            if (text == null)
            {
                throw new BadRequestException("Text does not exist.");
            }

            if (text.CreatorId != userId)
            {
                throw new ForbiddenException("You don't have permission to access this text.");
            }

            var readModel = new TextReadModel();
            readModel.Title = text.Title;
            readModel.Language = text.Language;
            readModel.Bookmark = text.Bookmark;
            readModel.Id = text.Id;
            var termViewModels = new List<TermReadModel>();
            ILanguage language = this.languageHelper.GetLanguage(text.Language);
            IEnumerable<string> notSkippedTerms =
                text.Words.Where(word => !language.ShouldSkip(word)).Select(t => language.Normalize(t));
            IDictionary<string, Term> termDict =
                await this.termRepository.GetManyAsync(userId, language.Id, notSkippedTerms.ToHashSet());

            foreach (string word in text.Words)
            {
                if (language.ShouldSkip(word))
                {
                    termViewModels.Add(new TermReadModel { Content = word, LearningLevel = TermLearningLevel.Skipped });
                    continue;
                }

                TermReadModel viewModel;
                string normalizedWord = language.Normalize(word);

                if (!termDict.ContainsKey(normalizedWord))
                {
                    viewModel = new TermReadModel { Content = word, LearningLevel = TermLearningLevel.UnKnow };
                }
                else
                {
                    Term term = termDict[normalizedWord];
                    viewModel = new TermReadModel
                    {
                        Id = term.Id, Content = word, LearningLevel = term.LearningLevel, Meaning = term.Meaning,
                    };
                }

                termViewModels.Add(viewModel);
            }

            readModel.Terms = termViewModels;

            return readModel;
        }

        /// <inheritdoc />
        public async Task<TextEditDetailModel> GetEditDetailAsync(Guid id, Guid userId)
        {
            Text text = await this.textRepository.GetByIdAsync(id);

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
        public async Task SetBookmarkAsync(Guid id, Guid userId, SetBookmarkModel setBookmarkModel)
        {
            Text text = await this.textRepository.GetByIdAsync(id);

            if (text == null)
            {
                throw new NotFoundException("Text not found.");
            }

            if (text.CreatorId != userId)
            {
                throw new ForbiddenException("You don't have permission to access this text.");
            }

            if (setBookmarkModel.TermIndex >= (ulong)text.Words.Count)
            {
                throw new BadRequestException("Invalid bookmark index.");
            }

            text.Bookmark = setBookmarkModel.TermIndex;

            if (!await this.textRepository.UpdateAsync(text))
            {
                throw new BadRequestException("Can not set bookmark.");
            }
        }

        private void SplitText(Text text)
        {
            ILanguage language = this.languageHelper.GetLanguage(text.Language);
            text.Words = language.SplitText(text.Content);
        }
    }
}