using System.Threading.Tasks;

namespace Lwt.Interfaces.Services
{
    public interface IUserService
    {
        Task SignUpAsync(string userName,string passWord);
        Task<bool> LoginAsync(string userName, string password);
        Task LogoutAsync();
    }
}
