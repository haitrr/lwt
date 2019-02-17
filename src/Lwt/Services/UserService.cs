namespace Lwt.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// a.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<User> userManager;

        private readonly ITokenProvider tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">userManager.</param>
        /// <param name="tokenProvider">authentication token provider.</param>
        public UserService(UserManager<User> userManager, ITokenProvider tokenProvider)
        {
            this.userManager = userManager;
            this.tokenProvider = tokenProvider;
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
        public async Task<string> LoginAsync(string userName, string password)
        {
            User user = await this.userManager.FindByNameAsync(userName);

            if (user != null)
            {
                if (await this.userManager.CheckPasswordAsync(user, password))
                {
                    return this.tokenProvider.GenerateUserToken(user);
                }
            }

            throw new BadRequestException("Username or password is incorrect.");
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(Guid userId, UserChangePasswordModel changePasswordModel)
        {
            User user = await this.userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            IdentityResult result = await this.userManager.ChangePasswordAsync(
                user,
                changePasswordModel.CurrentPassword,
                changePasswordModel.NewPassword);

            if (!result.Succeeded)
            {
                throw new BadRequestException("Current password not correct.");
            }
        }
    }
}