namespace Lwt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using LWT.Models;
    using Lwt.Repositories;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// a.
    /// </summary>
    public class TextRepository : BaseRepository<Text>, ITextRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextRepository"/> class.
        /// </summary>
        /// <param name="lwtDbContext">lwtDbContext.</param>
        public TextRepository(LwtDbContext lwtDbContext)
            : base(lwtDbContext)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Text>> GetByUserAsync(Guid userId)
        {
            return await this.DbSet.Where(t => t.UserId == userId).ToListAsync();
        }
    }
}