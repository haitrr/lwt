namespace Lwt.Mappers;

using Lwt.Models;
using Lwt.Services;

/// <inheritdoc/>
public class UserSettingViewMapper : BaseMapper<UserSetting, UserSettingView>
{
    /// <inheritdoc/>
    public override UserSettingView Map(UserSetting from, UserSettingView result)
    {
        result.UserId = from.UserId;
        result.LanguageSettings = from.LanguageSettings;
        return result;
    }
}