namespace Lwt.Repositories
{
    using System;
    using System.Threading.Tasks;

    using Lwt.Interfaces;
    using Lwt.Models;

    using Microsoft.AspNetCore.Identity;

    /// <inheritdoc />
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="userManager">user manager from entity framework.</param>
        public UserRepository(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        /// <inheritdoc />
        public async Task<bool> IsExistAsync(Guid id)
        {
            return await this.userManager.FindByIdAsync(id.ToString()) != null;
        }

        /// <inheritdoc />
        public async Task CreateAsync(User user, string password)
        {
            await this.userManager.CreateAsync(user, password);
        }

        /// <inheritdoc/>
        public Task<User> GetByIdAsync(Guid userId)
        {
            return this.userManager.FindByIdAsync(userId.ToString());
        }

        /// <inheritdoc/>
        public Task<User> GetByUserNameAsync(string userName)
        {
            return this.userManager.FindByNameAsync(userName);
        }
    }
}