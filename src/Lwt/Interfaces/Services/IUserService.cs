using System.Threading.Tasks;

namespace Lwt.Interfaces.Services
{
    public interface IUserService
    {
        Task<bool> SignUpAsync(string userName,string passWord);
        Task<bool> LoginAsync(string userName, string password);
        Task LogoutAsync();
    }
}
