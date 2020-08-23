namespace Lwt.Repositories
{
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    /// <summary>
    /// user setting repository.
    /// </summary>
    public class UserSettingRepository : BaseRepository<UserSetting>, IUserSettingRepository
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettingRepository"/> class.
    /// </summary>
    /// <param name="lwtDbContext"> the db context.</param>
    public UserSettingRepository(LwtDbContext lwtDbContext)
            : base(lwtDbContext)
        {
        }

    /// <inheritdoc/>
    public async Task<UserSetting?> TryGetByUserIdAsync(int userId)
        {
            return await this.Collection.AsQueryable().Where(u => u.UserId == userId).SingleOrDefaultAsync();
        }
    }
}