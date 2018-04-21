using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Interfaces.Services
{
    public interface IUserService
    {
        Task<bool> SignUpAsync(User newUser);
        Task<bool> LoginAsync(string userName, string password);
    }
}
