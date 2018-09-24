namespace Lwt.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Exceptions;
    using Lwt.Interfaces.Services;
    using Lwt.Models;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// a.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">userManager.</param>
        /// <param name="signInManager">signInManager.</param>
        public UserService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        /// <inheritdoc/>
        public async Task SignUpAsync(string userName, string passWord)
        {
            IdentityResult result = await this.userManager.CreateAsync(new User { UserName = userName }, passWord);

            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.First().Description);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> LoginAsync(string userName, string password)
        {
            SignInResult result = await this.signInManager.PasswordSignInAsync(userName, password, false, false);

            if (result.Succeeded)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task LogoutAsync()
        {
            await this.signInManager.SignOutAsync();
        }
    }
}