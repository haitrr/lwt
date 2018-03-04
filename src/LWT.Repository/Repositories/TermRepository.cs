using LWT.Data.Contexts;
using LWT.Data.Models;
using LWT.Repository.Interfaces;

namespace LWT.Repository.Repositories
{
    public class TermRepository : BaseRepository<Term>, ITermRepository
    {
        public TermRepository(LWTDbContext context) : base(context)
        {
        }
    }
}