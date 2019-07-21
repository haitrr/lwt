namespace Lwt.Mappers
{
    using Models;
    using Services;

    public class UserSettingUpdateMapper : BaseMapper<UserSettingUpdate, UserSetting>
    {
        public override UserSetting Map(UserSettingUpdate userSettingUpdate, UserSetting userSetting)
        {
            userSetting.LanguageSettings = userSettingUpdate.LanguageSettings;
            return userSetting;
        }
    }
}