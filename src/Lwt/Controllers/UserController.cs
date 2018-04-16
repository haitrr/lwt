using System;
using AutoMapper;
using Lwt.Interfaces.Services;
using Lwt.Models;
using Lwt.ViewModels.User;
using Microsoft.AspNetCore.Mvc;

namespace Lwt.Controllers
{
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserService _service;
        private readonly IMapper _mapper;

        public UserController(IUserService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpPost("sign-up")]
        public IActionResult SignUp(SignUpViewModel signUpViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var newUser = _mapper.Map<User>(signUpViewModel);
            if (newUser == null)
            {
                throw new NotSupportedException("Can not map to user.");
            }

            bool success = _service.SignUp(newUser);
            if (!success)
            {
                return BadRequest();
            }


            return Ok();
        }
    }
}
