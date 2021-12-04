using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Services;

public interface ITermEditor
{
    Task EditAsync(TermEditModel termEditModel, int termId, int userId);
}