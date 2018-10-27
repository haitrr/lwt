using Lwt.DbContexts;
using Lwt.Interfaces;
using Lwt.Models;

namespace Lwt.Repositories
{
    public class LanguageRepository : BaseRepository<Language>,ILanguageRepository
    {
        public LanguageRepository(LwtDbContext lwtDbContext) : base(lwtDbContext)
        {
        }
    }
}