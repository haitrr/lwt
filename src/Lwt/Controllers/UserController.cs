using System.Threading.Tasks;
using Lwt.Interfaces.Services;
using Lwt.ViewModels.User;
using Microsoft.AspNetCore.Mvc;

namespace Lwt.Controllers
{
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<IActionResult> SignUpAsync([FromBody] SignUpViewModel signUpViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }


            bool success = await _service.SignUpAsync(signUpViewModel.UserName, signUpViewModel.Password);
            if (!success)
            {
                return BadRequest();
            }


            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            bool success = await _service.LoginAsync(viewModel.UserName, viewModel.Password);
            if (success)
            {

                return Ok();
            }

            return BadRequest();
        }
    }
}
