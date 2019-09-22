namespace Lwt.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Lwt.Interfaces;
  using Lwt.Interfaces.Services;
  using Lwt.Models;
  using Lwt.ViewModels.User;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  /// <inheritdoc />
  /// <summary>
  /// a.
  /// </summary>
  [Route("api/user")]
  public class UserController : Controller
  {
    private readonly IUserService service;
    private readonly IAuthenticationHelper authenticationHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="service">service.</param>
    /// <param name="authenticationHelper">authentication helper.</param>
    public UserController(IUserService service, IAuthenticationHelper authenticationHelper)
    {
      this.service = service;
      this.authenticationHelper = authenticationHelper;
    }

    /// <summary>
    /// get logged in user profile.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
      Guid loggedInUserid = this.authenticationHelper.GetLoggedInUser(this.User);
      UserView user = await this.service.GetAsync(loggedInUserid);
      return this.Ok(user);
    }

    /// <summary>
    /// get logged in user setting.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("setting")]
    public async Task<IActionResult> GetSettingAsync()
    {
      Guid loggedInUserid = this.authenticationHelper.GetLoggedInUser(this.User);
      UserSettingView userSetting = await this.service.GetSettingAsync(loggedInUserid);
      return this.Ok(userSetting);
    }

    /// <summary>
    /// update logged in user settings.
    /// </summary>
    /// <param name="userSettingUpdate">user setting.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("setting")]
    public async Task<IActionResult> GetSettingAsync([FromBody] UserSettingUpdate userSettingUpdate)
    {
      Guid loggedInUserid = this.authenticationHelper.GetLoggedInUser(this.User);
      await this.service.PutSettingAsync(loggedInUserid, userSettingUpdate);
      return this.Ok();
    }

    /// <summary>
    /// a.
    /// </summary>
    /// <param name="signUpViewModel">signUpViewModel.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    public async Task<IActionResult> SignUpAsync([FromBody] SignUpViewModel signUpViewModel)
    {
      await this.service.SignUpAsync(signUpViewModel.UserName, signUpViewModel.Password);

      return this.Ok(new { });
    }

    /// <summary>
    /// login.
    /// </summary>
    /// <param name="viewModel">viewModel.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel viewModel)
    {
      if (!this.ModelState.IsValid)
      {
        return this.BadRequest();
      }

      string token = await this.service.LoginAsync(viewModel.UserName, viewModel.Password);

      return this.Ok(new LoginResult { Token = token });
    }

    /// <summary>
    /// change user password.
    /// </summary>
    /// <param name="changePasswordModel">change password model.</param>
    /// <returns>status.</returns>
    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] UserChangePasswordModel changePasswordModel)
    {
      Guid loggedInUserid = this.authenticationHelper.GetLoggedInUser(this.User);
      await this.service.ChangePasswordAsync(loggedInUserid, changePasswordModel);
      return this.Ok();
    }
  }
}