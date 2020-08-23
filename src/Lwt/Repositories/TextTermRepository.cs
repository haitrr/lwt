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

        public async Task<IEnumerable<TextTerm>> GetByTextAsync(int textId)
        {
            return await this.DbSet.Where(t => t.TextId == textId).Include(t => t.Term).ToListAsync();
        }
    }
}