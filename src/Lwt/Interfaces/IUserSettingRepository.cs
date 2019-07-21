namespace Lwt.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Models;

    public interface IUserSettingRepository : IRepository<UserSetting>
    {
        Task<UserSetting> GetByUserIdAsync(Guid userId);
    }
}