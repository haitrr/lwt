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
        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> SignUp(User newUser)
        {
            if (newUser == null)
            {
                throw new ArgumentNullException();
            }

            IdentityResult result = await _userManager.CreateAsync(newUser);
            return result.Succeeded;
        }
    }
}