namespace Lwt.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;

    public interface ITextTermRepository : ISqlRepository<TextTerm>
    {
        Task<int> CountByTextAsync(int textId);

        void DeleteByTextId(int textId);

        Task<IEnumerable<TextTerm>> GetByTextAsync(int textId, int? indexFrom, int? indexTo);

        Task<IEnumerable<TextTerm>> GetByUserAndLanguageAndContentAsync(
            int termCreatorId,
            LanguageCode termLanguageCode,
            string termContent);
    }
}