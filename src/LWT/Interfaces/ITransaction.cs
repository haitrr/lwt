using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Lwt.Interfaces
{
    public interface ITransaction
    {
        Task<bool> Commit();
    }
}
