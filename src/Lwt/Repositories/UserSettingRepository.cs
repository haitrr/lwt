namespace Lwt.Repositories
{
    using System;
    using System.Threading.Tasks;
    using DbContexts;
    using Interfaces;
    using Models;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    public class UserSettingRepository : BaseRepository<UserSetting>, IUserSettingRepository
    {
        public UserSettingRepository(LwtDbContext lwtDbContext)
            : base(lwtDbContext)
        {
        }

        public Task<UserSetting> GetByUserIdAsync(Guid userId)
        {
            return this.Collection.AsQueryable().Where(u => u.UserId == userId).SingleOrDefaultAsync();
        }
    }
}