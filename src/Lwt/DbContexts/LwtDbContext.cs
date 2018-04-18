using Microsoft.EntityFrameworkCore;

namespace Lwt.DbContexts
{
    public class LwtDbContext : DbContext
    {

        public LwtDbContext(DbContextOptions<LwtDbContext> options) : base(options)
        {

        }
    }
}
