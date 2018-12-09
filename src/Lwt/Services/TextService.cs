namespace Lwt.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentValidation;
    using FluentValidation.Results;

    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;

    /// <summary>
    /// the text service.
    /// </summary>
    public class TextService : ITextService
    {
        private readonly ITextRepository textRepository;

        private readonly ILanguageHelper languageHelper;

        private readonly IValidator<Text> textValidator;

        private readonly ITermRepository termRepository;

        private readonly IMapper<TextEditModel, Text> textEditMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextService"/> class.
        /// </summary>
        /// <param name="textRepository">textRepository.</param>
        /// <param name="textEditMapper">textEditMapper.</param>
        /// <param name="textValidator">textValidator.</param>
        /// <param name="languageHelper">the language helper.</param>
        /// <param name="termRepository">the term repository.</param>
        public TextService(
            ITextRepository textRepository,
            IMapper<TextEditModel, Text> textEditMapper,
            IValidator<Text> textValidator,
            ILanguageHelper languageHelper,
            ITermRepository termRepository)
        {
            this.textRepository = textRepository;
            this.textEditMapper = textEditMapper;
            this.textValidator = textValidator;
            this.languageHelper = languageHelper;
            this.termRepository = termRepository;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(Text text)
        {
            ValidationResult validationResult = this.textValidator.Validate(text);

            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
            }

            text.Words = this.languageHelper.GetLanguage(text.Language).SplitText(text.Content);
            await this.textRepository.AddAsync(text);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Text>> GetByUserAsync(
            Guid userId,
            TextFilter textFilter,
            PaginationQuery paginationQuery)
        {
            return this.textRepository.GetByUserAsync(userId, textFilter, paginationQuery);
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

            if (text.CreatorId == userId)
            {
                Text editedText = this.textEditMapper.Map(editModel, text);
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
            var termViewModels = new List<TermViewModel>();

            foreach (string word in text.Words)
            {
                Term term = await this.termRepository.GetByUserIdAndContentAsync(userId, word);
                TermViewModel viewModel;

                if (term == null)
                {
                    viewModel = new TermViewModel() { Content = word, LearningLevel = TermLearningLevel.UnKnow };
                }
                else
                {
                    viewModel = new TermViewModel()
                    {
                        Id = term.Id, Content = term.Content, LearningLevel = term.LearningLevel, Meaning = term.Meaning,
                    };
                }

                termViewModels.Add(viewModel);
            }

            readModel.Terms = termViewModels;

            return readModel;
        }
    }
}