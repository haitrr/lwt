namespace Lwt.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// a.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<User> userManager;

        private readonly ITokenProvider tokenProvider;
        private readonly IUserRepository userRepository;
        private readonly IMapper<User, UserView> userViewMapper;
        private readonly ISqlUserSettingRepository userSettingRepository;
        private readonly IMapper<UserSetting, UserSettingView> userSettingViewMapper;
        private readonly IMapper<UserSettingUpdate, UserSetting> userSettingUpdateMapper;
        private readonly IUserPasswordChanger userPasswordChanger;
        private readonly IDbTransaction dbTransaction;

        public UserService(
            UserManager<User> userManager,
            ITokenProvider tokenProvider,
            IMapper<User, UserView> userViewMapper,
            ISqlUserSettingRepository userSettingRepository,
            IMapper<UserSetting, UserSettingView> userSettingViewMapper,
            IMapper<UserSettingUpdate, UserSetting> userSettingUpdateMapper,
            IUserPasswordChanger userPasswordChanger,
            IUserRepository userRepository,
            IDbTransaction dbTransaction)
        {
            this.userManager = userManager;
            this.tokenProvider = tokenProvider;
            this.userViewMapper = userViewMapper;
            this.userSettingRepository = userSettingRepository;
            this.userSettingViewMapper = userSettingViewMapper;
            this.userSettingUpdateMapper = userSettingUpdateMapper;
            this.userPasswordChanger = userPasswordChanger;
            this.userRepository = userRepository;
            this.dbTransaction = dbTransaction;
        }

        /// <inheritdoc/>
        public async Task SignUpAsync(string userName, string passWord)
        {
            IdentityResult result = await this.userManager.CreateAsync(new User { UserName = userName }, passWord);

            if (!result.Succeeded)
            {
                throw new BadRequestException(
                    result.Errors.First()
                        .Description);
            }
        }

        /// <inheritdoc/>
        public async Task<string> LoginAsync(string userName, string password)
        {
            User user = await this.userManager.FindByNameAsync(userName);

            if (user != null)
            {
                if (await this.userManager.CheckPasswordAsync(user, password))
                {
                    return this.tokenProvider.GenerateUserToken(user);
                }
            }

            throw new BadRequestException("Username or password is incorrect.");
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(int userId, UserChangePasswordModel changePasswordModel)
        {
            bool exist = await this.userRepository.IsExistAsync(userId);

            if (!exist)
            {
                throw new NotFoundException("User not found.");
            }

            bool result = await this.userPasswordChanger.ChangePasswordAsync(
                userId,
                changePasswordModel.CurrentPassword,
                changePasswordModel.NewPassword);

            if (!result)
            {
                throw new BadRequestException("Current password is not correct.");
            }
        }

        /// <inheritdoc/>
        public async Task<UserView> GetAsync(int loggedInUserid)
        {
            User? user = await this.userManager.FindByIdAsync(loggedInUserid.ToString());

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            return this.userViewMapper.Map(user);
        }

        /// <inheritdoc/>
        public async Task<UserSettingView> GetSettingAsync(int loggedInUserid)
        {
            UserSetting? userSetting = await this.userSettingRepository.TryGetByUserIdAsync(loggedInUserid);

            if (userSetting == null)
            {
                throw new NotFoundException("User has no setting.");
            }

            return this.userSettingViewMapper.Map(userSetting);
        }

        /// <inheritdoc/>
        public async Task PutSettingAsync(int loggedInUserid, UserSettingUpdate userSettingUpdate)
        {
            // not including language setting to avoid tracking issue.
            UserSetting? userSetting = await this.userSettingRepository.TryGetByUserIdNotIncludeLanguageSettingsAsync(loggedInUserid);

            if (userSetting == null)
            {
                UserSetting newUserSetting = this.userSettingUpdateMapper.Map(userSettingUpdate);
                newUserSetting.UserId = loggedInUserid;
                this.userSettingRepository.Add(newUserSetting);
            }
            else
            {
                UserSetting updatedUserSetting = this.userSettingUpdateMapper.Map(userSettingUpdate, userSetting);
                this.userSettingRepository.Update(updatedUserSetting);
            }

            await this.dbTransaction.CommitAsync();
        }
    }
}