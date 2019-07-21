namespace Lwt.Mappers
{
    using Lwt.Models;
    using Lwt.Services;

    public class UserViewMapper : BaseMapper<User, UserView>
    {
        public override UserView Map(User user, UserView userView)
        {
            userView.UserName = user.UserName;
            userView.Email = user.Email;
            return userView;
        }
    }
}