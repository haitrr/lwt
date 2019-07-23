namespace Lwt.Services
{
    using System;
    using System.Collections;
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
        private readonly IMapper<User, UserView> userViewMapper;
        private readonly IUserSettingRepository userSettingRepository;
        private readonly IMapper<UserSetting, UserSettingView> userSettingViewMapper;
        private readonly IMapper<UserSettingUpdate, UserSetting> userSettingUpdateMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">userManager.</param>
        /// <param name="tokenProvider">authentication token provider.</param>
        public UserService(
            UserManager<User> userManager,
            ITokenProvider tokenProvider,
            IMapper<User, UserView> userViewMapper,
            IUserSettingRepository userSettingRepository,
            IMapper<UserSetting, UserSettingView> userSettingViewMapper,
            IMapper<UserSettingUpdate, UserSetting> userSettingUpdateMapper)
        {
            this.userManager = userManager;
            this.tokenProvider = tokenProvider;
            this.userViewMapper = userViewMapper;
            this.userSettingRepository = userSettingRepository;
            this.userSettingViewMapper = userSettingViewMapper;
            this.userSettingUpdateMapper = userSettingUpdateMapper;
        }

        /// <inheritdoc/>
        public async Task SignUpAsync(string userName, string passWord)
        {
            IdentityResult result = await this.userManager.CreateAsync(new User { UserName = userName }, passWord);

            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.First().Description);
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
        public async Task ChangePasswordAsync(Guid userId, UserChangePasswordModel changePasswordModel)
        {
            User user = await this.userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            IdentityResult result = await this.userManager.ChangePasswordAsync(
                user,
                changePasswordModel.CurrentPassword,
                changePasswordModel.NewPassword);

            if (!result.Succeeded)
            {
                throw new BadRequestException("Current password not correct.");
            }
        }

        public async Task<UserView> GetAsync(Guid loggedInUserid)
        {
            User user = await this.userManager.FindByIdAsync(loggedInUserid.ToString());

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            return this.userViewMapper.Map(user);
        }

        public async Task<UserSettingView> GetSettingAsync(Guid loggedInUserid)
        {
            UserSetting userSetting = await this.userSettingRepository.GetByUserIdAsync(loggedInUserid);

            if (userSetting == null)
            {
                throw new NotFoundException("User has no setting.");
            }

            return this.userSettingViewMapper.Map(userSetting);
        }

        public async Task PutSettingAsync(Guid loggedInUserid, UserSettingUpdate userSettingUpdate)
        {
            UserSetting userSetting = await this.userSettingRepository.GetByUserIdAsync(loggedInUserid);

            if (userSetting == null)
            {
                UserSetting newUserSetting = this.userSettingUpdateMapper.Map(userSettingUpdate);
                newUserSetting.UserId = loggedInUserid;
                await this.userSettingRepository.AddAsync(newUserSetting);
                return;
            }

            UserSetting updatedUserSetting = this.userSettingUpdateMapper.Map(userSettingUpdate, userSetting);
            await this.userSettingRepository.UpdateAsync(updatedUserSetting);
        }
    }
}