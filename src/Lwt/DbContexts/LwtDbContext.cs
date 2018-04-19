using Lwt.Models;
using Microsoft.EntityFrameworkCore;

namespace Lwt.DbContexts
{
    public class LwtDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public LwtDbContext(DbContextOptions<LwtDbContext> options) : base(options)
        {
        }
    }
}
