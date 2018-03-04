

using LWT.Data.Contexts;
using LWT.Data.Models;
using LWT.Repository.Interfaces;

namespace LWT.Repository.Repositories
{
    public class LanguageRepository : BaseRepository<Language>, ILanguageRepository
    {
        public LanguageRepository(LWTDbContext context) : base(context)
        {

        }
    }
}