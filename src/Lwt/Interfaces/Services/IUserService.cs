using Lwt.Models;

namespace Lwt.Interfaces.Services
{
    public interface IUserService
    {
        bool SignUp(User newUser);
    }
}
