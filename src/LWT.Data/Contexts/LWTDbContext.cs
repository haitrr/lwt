using Microsoft.EntityFrameworkCore;

namespace LWT.Data.Contexts
{
    // ReSharper disable once InconsistentNaming
    public class LWTDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // define the database to use
            optionsBuilder.UseInMemoryDatabase("LWT");
        }
    }
}