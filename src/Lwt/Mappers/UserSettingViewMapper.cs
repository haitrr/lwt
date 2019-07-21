namespace Lwt.Mappers
{
    using Models;
    using Services;

    public class UserSettingViewMapper : BaseMapper<UserSetting, UserSettingView>
    {
        public override UserSettingView Map(UserSetting userSetting, UserSettingView userSettingView)
        {
            userSettingView.UserId = userSetting.UserId;
            userSettingView.LanguageSettings = userSetting.LanguageSettings;
            return userSettingView;
        }
    }
}