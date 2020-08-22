namespace Lwt.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
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

        /// <inheritdoc />
        public async Task<IDictionary<string, Term>> GetManyAsync(
            Guid creatorId,
            LanguageCode languageCode,
            HashSet<string> terms)
        {
            return await this.DbSet.Where(
                    t => t.CreatorId == creatorId && t.LanguageCode == languageCode && terms.Contains(t.Content))
                .ToDictionaryAsync(t => t.Content, t => t);
        }

        /// <inheritdoc />
        public Task<Term> GetUserTermAsync(Guid termId, Guid userId)
        {
            return this.DbSet.SingleAsync(t => t.CreatorId == userId && t.Id == termId);
        }
    }
}