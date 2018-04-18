using Lwt.Models;

namespace Lwt.Interfaces.Services
{
    public interface IUserService
    {
        bool SignUp(User newUser);
    }

    public class UserService : IUserService
    {
        public bool SignUp(User newUser)
        {
            throw new System.NotImplementedException();
        }
    }
}
