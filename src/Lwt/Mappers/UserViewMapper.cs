namespace Lwt.Mappers;

using Lwt.Models;
using Lwt.Services;

/// <inheritdoc/>
public class UserViewMapper : BaseMapper<User, UserView>
{
    /// <inheritdoc/>
    public override UserView Map(User from, UserView result)
    {
        result.UserName = from.UserName;
        result.Email = from.Email;
        return result;
    }
}