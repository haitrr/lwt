namespace Lwt.Services
{
  using System;
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
    private readonly IUserSettingRepository userSettingRepository;
    private readonly IMapper<UserSetting, UserSettingView> userSettingViewMapper;
    private readonly IMapper<UserSettingUpdate, UserSetting> userSettingUpdateMapper;
    private readonly IUserPasswordChanger userPasswordChanger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="userManager">userManager.</param>
    /// <param name="tokenProvider">authentication token provider.</param>
    /// <param name="userViewMapper">user view mapper.</param>
    /// <param name="userSettingRepository">user setting repository.</param>
    /// <param name="userSettingViewMapper">user setting view mapper.</param>
    /// <param name="userSettingUpdateMapper">user setting update mapper.</param>
    /// <param name="userPasswordChanger">user password changer.</param>
    /// <param name="userRepository">user repository.</param>
    public UserService(
            UserManager<User> userManager,
            ITokenProvider tokenProvider,
            IMapper<User, UserView> userViewMapper,
            IUserSettingRepository userSettingRepository,
            IMapper<UserSetting, UserSettingView> userSettingViewMapper,
            IMapper<UserSettingUpdate, UserSetting> userSettingUpdateMapper,
            IUserPasswordChanger userPasswordChanger,
            IUserRepository userRepository)
    {
      this.userManager = userManager;
      this.tokenProvider = tokenProvider;
      this.userViewMapper = userViewMapper;
      this.userSettingRepository = userSettingRepository;
      this.userSettingViewMapper = userSettingViewMapper;
      this.userSettingUpdateMapper = userSettingUpdateMapper;
      this.userPasswordChanger = userPasswordChanger;
      this.userRepository = userRepository;
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
    public async Task<UserView> GetAsync(Guid loggedInUserid)
    {
      User? user = await this.userManager.FindByIdAsync(loggedInUserid.ToString());

      if (user == null)
      {
        throw new NotFoundException("User not found.");
      }

      return this.userViewMapper.Map(user);
    }

    /// <inheritdoc/>
    public async Task<UserSettingView> GetSettingAsync(Guid loggedInUserid)
    {
      UserSetting userSetting = await this.userSettingRepository.TryGetByUserIdAsync(loggedInUserid);

      if (userSetting == null)
      {
        throw new NotFoundException("User has no setting.");
      }

      return this.userSettingViewMapper.Map(userSetting);
    }

    /// <inheritdoc/>
    public async Task PutSettingAsync(Guid loggedInUserid, UserSettingUpdate userSettingUpdate)
    {
      UserSetting? userSetting = await this.userSettingRepository.TryGetByUserIdAsync(loggedInUserid);

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