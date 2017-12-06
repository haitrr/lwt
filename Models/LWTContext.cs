using Microsoft.EntityFrameworkCore;

namespace LWT.Models
{
    class LWTContext : DbContext
    {
        public LWTContext(DbContextOptions<LWTContext> options):base(options)
        {
        }
        public DbSet<Text> Texts { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Term> Terms { get; set; }
    }
}