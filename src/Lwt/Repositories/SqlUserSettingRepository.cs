namespace Lwt.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// user setting repository.
    /// </summary>
    public class SqlUserSettingRepository : BaseSqlRepository<UserSetting>, ISqlUserSettingRepository
    {
        public SqlUserSettingRepository(IdentityDbContext identityDbContext)
            : base(identityDbContext)
        {
        }

        /// <inheritdoc/>
        public async Task<UserSetting?> TryGetByUserIdAsync(int userId)
        {
            return await this.DbSet.Where(u => u.UserId == userId)
                .SingleOrDefaultAsync();
        }
    }
}