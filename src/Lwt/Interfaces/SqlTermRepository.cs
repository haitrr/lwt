namespace Lwt.Interfaces
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Exceptions;
    using Lwt.Models;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc cref="Lwt.Interfaces.ISqlTermRepository" />
    public class SqlTermRepository : BaseSqlRepository<Term>, ISqlTermRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTermRepository"/> class.
        /// </summary>
        /// <param name="identityDbContext">db context.</param>
        public SqlTermRepository(IdentityDbContext identityDbContext)
            : base(identityDbContext)
        {
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, LearningLevel>> GetLearningLevelAsync(
            int creatorId,
            LanguageCode languageCode,
            ISet<string> terms)
        {
            var list = await this.DbSet.Where(
                    t => terms.Contains(t.Content) && t.CreatorId == creatorId && t.LanguageCode == languageCode)
                .Select(t => new { t.Content, t.LearningLevel })
                .ToListAsync();
            return list.ToDictionary(t => t.Content, t => t.LearningLevel);
        }

        /// <inheritdoc />
        public async Task<IDictionary<string, Term>> GetManyAsync(
            int creatorId,
            LanguageCode languageCode,
            HashSet<string> terms)
        {
            return await this.DbSet.Where(
                    t => t.CreatorId == creatorId && t.LanguageCode == languageCode && terms.Contains(t.Content))
                .ToDictionaryAsync(t => t.Content, t => t);
        }

        /// <inheritdoc />
        public async Task<Term> GetUserTermAsync(int termId, int userId)
        {
            Term? term = await this.DbSet.SingleOrDefaultAsync(t => t.CreatorId == userId && t.Id == termId);

            if (term != null)
            {
                return term;
            }

            throw new NotFoundException("term not found");
        }

        /// <inheritdoc />
        public Task<Term?> TryGetByUserAndLanguageAndContentAsync(int userId, LanguageCode languageCode, string word)
        {
            return this.DbSet.Where(
                    term => term.Content == word.ToUpperInvariant() && term.LanguageCode == languageCode &&
                            term.CreatorId == userId)
                .SingleOrDefaultAsync() !;
        }
    }
}