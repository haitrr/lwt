using System;
using Xunit;

namespace Lwt.Test {
    public class UserControllerTest {
        private Mock<IUserSearvice> _userService;
        private Mock<IMapper> _mapper;
        private UserController _userController;
        public UserControllerTest () {
            _userService = new Mock<IUserSearvice> ();
            _mapper = new Mock<IMapper> ();
            _userController = new UserController (_userService.Object, _userController.Object);
        }

        [Fact]
        public void Test1 () {

        }
    }
}