
using LWT.Data.Models;
using LWT.Data.Contexts;
using LWT.Repository.Interfaces;

namespace LWT.Repository.Repositories
{
    public class TermRepository : Repository<Term>,ITermRepository
    {
        public TermRepository(LWTContext context) : base(context)
        {

        }
    }
}