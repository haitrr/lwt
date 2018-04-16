using System;
using AutoMapper;
using Lwt.Controllers;
using Lwt.Interfaces.Services;
using Lwt.Models;
using Lwt.ViewModels.User;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace Lwt.Test.Controllers
{
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _userService;
        private readonly Mock<IMapper> _mapper;
        private readonly UserController _userController;
        public UserControllerTest()
        {
            _userService = new Mock<IUserService>();
            _mapper = new Mock<IMapper>();
            _userController = new UserController(_userService.Object, _mapper.Object);
        }

        [Fact]
        public void SignUp_ShouldReturnBadRequest_IfViewModelNotValid()
        {
            // arrange
            _userController.ModelState.AddModelError("error", "message");

            // act
            var result = _userController.SignUp(null);

            // asert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void SignUp_ShouldReturnThrowException_IfCantMap()
        {
            // arrange
            _mapper.Setup(m => m.Map<User>(It.IsAny<SignUpViewModel>())).Returns((User)null);
            var viewModel = new SignUpViewModel();

            // act

            // asert
            Assert.Throws<NotSupportedException>(() => _userController.SignUp(viewModel));
        }

        [Fact]
        public void SignUp_ShouldReturnOk_IfSignUpSuccess()
        {
            // arrange
            var signUpViewModel = new SignUpViewModel();
            var user = new User();
            _mapper.Setup(m => m.Map<User>(signUpViewModel)).Returns(user);
            _userService.Setup(s => s.SignUp(user)).Returns(true);


            // act
            var result = _userController.SignUp(signUpViewModel);

            // assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void SignUp_ShouldReturnBadRequest_IfSignUpFail()
        {
            // arrange
            var signUpViewModel = new SignUpViewModel();
            var user = new User();

            _mapper.Setup(m => m.Map<User>(signUpViewModel)).Returns(user);
            _userService.Setup(s => s.SignUp(user)).Returns(false);

            // act
            var result = _userController.SignUp(signUpViewModel);

            // assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}