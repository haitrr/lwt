using System.Threading.Tasks;
using Lwt.Interfaces.Services;
using Lwt.Models;
using Microsoft.AspNetCore.Identity;

namespace Lwt.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public UserService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<bool> SignUpAsync(string userName, string passWord)
        {
            IdentityResult result = await _userManager.CreateAsync(new User { UserName = userName }, passWord);
            return result.Succeeded;
        }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            SignInResult result = await _signInManager.PasswordSignInAsync(userName, password, false, false);

            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}