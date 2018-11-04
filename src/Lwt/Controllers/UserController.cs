namespace Lwt.Controllers
{
    using System.Threading.Tasks;
    using Lwt.Interfaces.Services;
    using Lwt.ViewModels.User;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// a.
    /// </summary>
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="service">service.</param>
        public UserController(IUserService service)
        {
            this.service = service;
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

            return this.Ok();
        }

        /// <summary>
        /// a.
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

            bool success = await this.service.LoginAsync(viewModel.UserName, viewModel.Password);

            if (success)
            {
                return this.Ok(new {});
            }

            return this.BadRequest();
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutAsync()
        {
            await this.service.LogoutAsync();
            return this.Ok(new {});
        }
    }
}