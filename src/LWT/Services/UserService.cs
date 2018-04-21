using System;
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

        public async Task<bool> SignUpAsync(User newUser)
        {
            if (newUser == null)
            {
                throw new ArgumentNullException();
            }

            IdentityResult result = await _userManager.CreateAsync(newUser);
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
    }
}