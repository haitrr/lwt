using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Interfaces.Services
{
    public interface IUserService
    {
        Task<bool> SignUp(User newUser);
    }
}
