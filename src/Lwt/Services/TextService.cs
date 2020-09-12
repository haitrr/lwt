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
        private readonly IMapper<TextTerm, TermReadModel> textTermMapper;

        public TextService(
            ISqlTextRepository textRepository,
            IMapper<TextEditModel, Text> textEditMapper,
            IMapper<Text, TextViewModel> textViewMapper,
            IMapper<Text, TextEditDetailModel> textEditDetailMapper,
            ITextCreator textCreator,
            IMapper<Text, TextReadModel> textReadMapper,
            IDbTransaction dbTransaction,
            ITextTermRepository textTermRepository,
            IMapper<TextTerm, TermReadModel> textTermMapper)
        {
            this.textRepository = textRepository;
            this.textEditMapper = textEditMapper;
            this.textViewMapper = textViewMapper;
            this.textEditDetailMapper = textEditDetailMapper;
            this.textCreator = textCreator;
            this.textReadMapper = textReadMapper;
            this.dbTransaction = dbTransaction;
            this.textTermRepository = textTermRepository;
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

                editedText.ProcessedTermCount = 0;
                editedText.TermCount = 0;
                this.textRepository.Update(editedText);
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
                    t => new Text()
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Bookmark = t.Bookmark,
                        LanguageCode = t.LanguageCode,
                        TermCount = t.TermCount,
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
            IQueryable<Text> query = this.textRepository.Queryable()
                .AsNoTracking()
                .Where(t => t.Id == id && t.UserId == userId)
                .Select(t => new Text { Bookmark = t.Bookmark, Id = t.Id, TermCount = t.TermCount });
            Text? text = await query.FirstOrDefaultAsync();

            if (text == null)
            {
                throw new ForbiddenException("You don't have permission to access this text.");
            }

            if (setBookmarkModel.TermIndex >= (ulong)text.TermCount)
            {
                throw new BadRequestException("Invalid bookmark index.");
            }

            this.textRepository.UpdateBookmarkAsync(text.Id, setBookmarkModel.TermIndex);
            await this.dbTransaction.CommitAsync();
        }

        public async Task<IDictionary<LearningLevel, int>> GetTermCountsAsync(int id, int userId)
        {
            var groups = await this.textTermRepository.Queryable()
                .AsNoTracking()
                .Where(t => t.TextId == id && t.Text.UserId == userId)
                .GroupBy(t => t.Term!.LearningLevel)
                .Select(group => new { LearningLevel = group.Key, Count = group.Sum(t => 1) })
                .ToListAsync();
            var result = new Dictionary<LearningLevel, int>()
            {
                { LearningLevel.Skipped, 0 },
                { LearningLevel.Ignored, 0 },
                { LearningLevel.Unknown, 0 },
                { LearningLevel.Learning1, 0 },
                { LearningLevel.Learning2, 0 },
                { LearningLevel.Learning3, 0 },
                { LearningLevel.Learning4, 0 },
                { LearningLevel.Learning5, 0 },
                { LearningLevel.WellKnown, 0 },
            };

            foreach (var group in groups)
            {
                if (group.LearningLevel == null)
                {
                    result[LearningLevel.Skipped] += group.Count;
                }
                else
                {
                    result[group.LearningLevel] += group.Count;
                }
            }

            return result;
        }

        public async Task<IEnumerable<TermReadModel>> GetTextTermsAsync(int id, int userId, int indexFrom, int indexTo)
        {
            IQueryable<TextTerm> query = this.textTermRepository.Queryable()
                .AsNoTracking()
                .Where(t => t.Text.UserId == userId && t.TextId == id && t.Index >= indexFrom && t.Index <= indexTo)
                .Select(
                    t => new TextTerm
                    {
                        Content = t.Content,
                        TermId = t.TermId,
                        TextId = id,
                        Index = t.Index,
                        Term = t.TermId.HasValue
                            ? new Term { LearningLevel = t.Term!.LearningLevel, Meaning = t.Term.Meaning }
                            : null,
                    });
            IEnumerable<TextTerm> textTerms = await query.ToListAsync();
            HashSet<int?> termToCount = textTerms.Where(t => t.TermId != null)
                .Select(t => t.TermId)
                .ToHashSet();
            Dictionary<int?, int> termCounts = this.textTermRepository.Queryable()
                .Where(t => t.TextId == id && termToCount.Contains(t.TermId))
                .GroupBy(t => t.TermId)
                .Select(t => new { Id = t.Key, Count = t.Count() })
                .ToDictionary(t => t.Id, t => t.Count);
            ICollection<TermReadModel> result =  this.textTermMapper.Map(textTerms);

            foreach (TermReadModel termReadModel in result.Where(r => r.Id.HasValue))
            {
                termReadModel.Count = termCounts[termReadModel.Id.Value];
            }

            return result;
        }

        public async Task<int> GetTermCountInTextAsync(int id, int userId, int termId)
        {
            return await this.textTermRepository.Queryable()
                .Where(t => t.TextId == id && t.Text.UserId == userId && t.TermId == termId)
                .CountAsync();
        }

        public async Task<int> GetProcessedTermCountAsync(int id, int userId)
        {
            int? count = await this.textRepository.Queryable()
                .Where(t => t.Id == id && t.UserId == userId)
                .Select(t => t.ProcessedTermCount)
                .FirstOrDefaultAsync();

            if (count == null)
            {
                throw new NotFoundException("Text not found.");
            }

            return count.Value;
        }

        public async Task<long> GetTermCountAsync(int id, int userId)
        {
            long? count = await this.textRepository.Queryable()
                .Where(t => t.Id == id && t.UserId == userId)
                .Select(t => t.TermCount)
                .FirstOrDefaultAsync();

            if (count == null)
            {
                throw new NotFoundException("Text not found.");
            }

            return count.Value;
        }
    }
}