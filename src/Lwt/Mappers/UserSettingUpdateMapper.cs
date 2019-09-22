namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    /// <inheritdoc/>
    public class UserSettingUpdateMapper : BaseMapper<UserSettingUpdate, UserSetting>
    {
        /// <inheritdoc/>
        public override UserSetting Map(UserSettingUpdate from, UserSetting result)
        {
            result.LanguageSettings = from.LanguageSettings;
            return result;
        }
    }
}