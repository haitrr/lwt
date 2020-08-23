namespace Lwt.Repositories
{
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;

    public interface ITextTermRepository : ISqlRepository<TextTerm>
    {
        Task<int> CountByTextAsync(int textId);

        void DeleteByTextId(int textId);
    }
}