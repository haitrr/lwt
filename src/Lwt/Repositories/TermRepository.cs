namespace Lwt.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using MongoDB.Driver;

    /// <summary>
    /// the term repository.
    /// </summary>
    public class TermRepository : BaseRepository<Term>, ITermRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TermRepository"/> class.
        /// </summary>
        /// <param name="lwtDbContext">the application db context.</param>
        public TermRepository(LwtDbContext lwtDbContext)
            : base(lwtDbContext)
        {
        }

        /// <inheritdoc />
        public Task<Term> GetByUserAndLanguageAndContentAsync(Guid userId, Language language, string word)
        {
            return this.Collection
                .Find(
                    term => term.Content == word.ToUpperInvariant() && term.Language == language &&
                            term.CreatorId == userId).SingleOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, TermLearningLevel>> GetLearningLevelAsync(
            Guid creatorId,
            Language language,
            ISet<string> terms)
        {
            var list = await this.Collection
                .Find(t => terms.Contains(t.Content) && t.CreatorId == creatorId && t.Language == language)
                .Project(t => new { t.Content, t.LearningLevel }).ToListAsync();
            return list.ToDictionary(t => t.Content, t => t.LearningLevel);
        }

        /// <inheritdoc />
        public async Task<IDictionary<string, Term>> GetManyAsync(
            Guid creatorId,
            Language language,
            HashSet<string> terms)
        {
            List<Term> list = await this.Collection
                .Find(t => terms.Contains(t.Content) && t.CreatorId == creatorId && t.Language == language)
                .ToListAsync();
            return list.ToDictionary(t => t.Content, t => t);
        }

        /// <inheritdoc />
        public override Task AddAsync(Term term)
        {
            // normalize the term's content before insert.
            term.Content = term.Content.ToUpperInvariant();
            return base.AddAsync(term);
        }
    }
}