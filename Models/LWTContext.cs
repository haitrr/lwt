using Microsoft.EntityFrameworkCore;

namespace LWT.Models
{
    public class LWTContext : DbContext
    {
        public LWTContext(DbContextOptions<LWTContext> options):base(options)
        {
        }
        public DbSet<Text> Text { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<Term> Term { get; set; }
    }
}