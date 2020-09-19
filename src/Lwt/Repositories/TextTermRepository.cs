namespace Lwt.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.EntityFrameworkCore;

    public class TextTermRepository : BaseSqlRepository<TextTerm>, ITextTermRepository
    {
        public TextTermRepository(IdentityDbContext identityDbContext)
            : base(identityDbContext)
        {
        }

        public Task<int> CountByTextAsync(int textId)
        {
            return this.DbSet.CountAsync(tt => tt.TextId == textId);
        }

        public void DeleteByTextId(int textId)
        {
            foreach (var textTerm in this.DbSet.Where(tt => tt.TextId == textId))
            {
                this.DbSet.Remove(textTerm);
            }
        }

        public async Task<IEnumerable<TextTerm>> GetByTextAsync(int textId, int? indexFrom, int? indexTo)
        {
            IQueryable<TextTerm> query = this.DbSet.Include(t => t.Term)
                .Where(t => t.TextId == textId);

            if (indexFrom != null)
            {
                query = query.Where(t => t.IndexFrom >= indexFrom);
            }

            if (indexTo != null)
            {
                query = query.Where(t => t.IndexTo <= indexTo);
            }

            return await query.OrderBy(t => t.IndexFrom)
                .ToListAsync();
        }

        public async Task<IEnumerable<TextTerm>> GetByUserAndLanguageAndContentAsync(
            int termCreatorId,
            LanguageCode termLanguageCode,
            string termContent)
        {
            return await this.DbSet.Where(
                    t => t.Text.UserId == termCreatorId && t.Text.LanguageCode == termLanguageCode &&
                         t.Content.ToUpper() == termContent.ToUpper())
                .ToListAsync();
        }

        public async Task<IDictionary<LearningLevel, int>> CountTextTermByLearningLevelAsync(int textId)
        {
            var groups = await this.DbSet.Where(tt => tt.TextId == textId)
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

        public Task<int> GetTermCountInTextAsync(int textId, int termId)
        {
            return this.DbSet.CountAsync(tt => tt.TextId == textId && tt.TermId == termId);
        }
    }
}