namespace Lwt.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Models;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    /// <inheritdoc/>
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
    public Task<UserSetting> GetByUserIdAsync(Guid userId)
        {
            return this.Collection.AsQueryable().Where(u => u.UserId == userId).SingleOrDefaultAsync();
        }
    }
}