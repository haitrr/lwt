namespace Lwt.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Exceptions;
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
        public async Task<Dictionary<string, LearningLevel>> GetLearningLevelAsync(
            Guid creatorId,
            LanguageCode languageCode,
            ISet<string> terms)
        {
            var list = await this.Collection.Find(
                    t => terms.Contains(t.Content) && t.CreatorId == creatorId && t.LanguageCode == languageCode)
                .Project(t => new { t.Content, t.LearningLevel })
                .ToListAsync();
            return list.ToDictionary(t => t.Content, t => t.LearningLevel);
        }

        /// <inheritdoc />
        public async Task<IDictionary<string, Term>> GetManyAsync(
            Guid creatorId,
            LanguageCode languageCode,
            HashSet<string> terms)
        {
            List<Term> list = await this.Collection.Find(
                    t => terms.Contains(t.Content) && t.CreatorId == creatorId && t.LanguageCode == languageCode)
                .ToListAsync();
            return list.ToDictionary(t => t.Content, t => t);
        }

        /// <inheritdoc/>
        public async Task<Term> GetUserTermAsync(Guid termId, Guid userId)
        {
            Term? term = await this.Collection.Find(t => t.Id == termId && t.CreatorId == userId)
                .SingleOrDefaultAsync();

            if (term == null)
            {
                throw new NotFoundException("Term not found");
            }

            return term;
        }

        /// <inheritdoc />
        public override Task AddAsync(Term entity)
        {
            // normalize the term's content before insert.
            entity.Content = entity.Content.ToUpperInvariant();
            return base.AddAsync(entity);
        }
    }
}