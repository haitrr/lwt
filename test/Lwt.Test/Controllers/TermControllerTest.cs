using System;
using System.Threading.Tasks;
using Lwt.Controllers;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Lwt.Test.Controllers
{
    public class TermControllerTest
    {
        private readonly TermController _termController;
        private readonly Mock<ITermService> _termService;
        private readonly Mock<IMapper<TermCreateModel, Guid, Term>> _termCreateMapper;
        private readonly Mock<IAuthenticationHelper> _authenticationHelper;

        public TermControllerTest()
        {
            _termService = new Mock<ITermService>();
            _termCreateMapper = new Mock<IMapper<TermCreateModel, Guid, Term>>();
            _authenticationHelper = new Mock<IAuthenticationHelper>();
            _termController = new TermController(_termService.Object, _termCreateMapper.Object,
                _authenticationHelper.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnOkTermId()
        {
            // arrange
            var termId = Guid.NewGuid();
            _termService.Setup(s => s.CreateAsync(It.IsAny<Term>())).ReturnsAsync(termId);
            //act
            var actual = await _termController.CreateAsync(new TermCreateModel());

            // assert
            var obj = Assert.IsType<OkObjectResult>(actual);
            Guid id = Assert.IsType<Guid>(obj.Value);
            Assert.Equal(termId, id);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallService()
        {
            // arrange
            var userId = Guid.NewGuid();
            var term = new Term();
            _authenticationHelper.Setup(h => h.GetLoggedInUser(_termController.User)).Returns(userId);
            var termCreateModel = new TermCreateModel();

            _termCreateMapper.Setup(m => m.Map(termCreateModel, userId)).Returns(term);
            //act
            await _termController.CreateAsync(termCreateModel);

            //assert
            _termService.Verify(s => s.CreateAsync(term), Times.Once);
        }
    }
}