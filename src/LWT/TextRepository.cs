using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lwt.DbContexts;
using Lwt.Interfaces;
using Lwt.Repositories;
using LWT.Models;
using Microsoft.EntityFrameworkCore;

namespace Lwt
{
    public class TextRepository : BaseRepository<Text>, ITextRepository
    {
        public TextRepository(LwtDbContext lwtDbContext) : base(lwtDbContext)
        {
        }

        public async Task<IEnumerable<Text>> GetByUserAsync(Guid userId)
        {
            return await DbSet.Where(t => t.UserId == userId).ToListAsync();
        }
    }
}