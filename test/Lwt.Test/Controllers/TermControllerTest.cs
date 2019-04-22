namespace Lwt.Test.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Controllers;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <inheritdoc />
    /// <summary>
    /// a.
    /// </summary>
    public class TermControllerTest : IDisposable
    {
        private readonly TermController termController;

        private readonly Mock<ITermService> termService;

        private readonly Mock<IMapper<TermCreateModel, Guid, Term>> termCreateMapper;

        private readonly Mock<IAuthenticationHelper> authenticationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermControllerTest"/> class.
        /// </summary>
        public TermControllerTest()
        {
            this.termService = new Mock<ITermService>();
            this.termCreateMapper = new Mock<IMapper<TermCreateModel, Guid, Term>>();
            this.authenticationHelper = new Mock<IAuthenticationHelper>();

            this.termController = new TermController(
                this.termService.Object,
                this.termCreateMapper.Object,
                this.authenticationHelper.Object);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TermControllerTest"/> class.
        /// </summary>
        ~TermControllerTest()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateAsyncShouldReturnOkTermId()
        {
            // arrange
            Guid termId = Guid.NewGuid();
            this.termService.Setup(s => s.CreateAsync(It.IsAny<Term>())).ReturnsAsync(termId);

            // act
            IActionResult actual = await this.termController.CreateAsync(new TermCreateModel());

            // assert
            var obj = Assert.IsType<OkObjectResult>(actual);
            var id = Assert.IsType<Guid>(obj.Value);
            Assert.Equal(termId, id);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateAsyncShouldCallService()
        {
            // arrange
            Guid userId = Guid.NewGuid();
            var term = new Term();
            this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.termController.User)).Returns(userId);
            var termCreateModel = new TermCreateModel();

            this.termCreateMapper.Setup(m => m.Map(termCreateModel, userId)).Returns(term);

            // act
            await this.termController.CreateAsync(termCreateModel);

            // assert
            this.termService.Verify(s => s.CreateAsync(term), Times.Once);
        }

        /// <summary>
        /// test search async return count.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetSearchAsyncShouldReturnCount()
        {
            long count = new Random().Next(1, 100000);
            var termFilter = new TermFilter();
            Guid userId = Guid.NewGuid();
            var paginationQuery = new PaginationQuery();
            this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.termController.User)).Returns(userId);
            this.termService.Setup(s => s.CountAsync(userId, termFilter)).ReturnsAsync(count);

            IActionResult rs = await this.termController.SearchAsync(termFilter, paginationQuery);
            var objectResult = Assert.IsType<OkObjectResult>(rs);
            var termList = Assert.IsType<TermList>(objectResult.Value);
            Assert.Equal(count, termList.Total);
        }

        /// <summary>
        /// test search async return count.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetSearchAsyncShouldReturnTermViewModels()
        {
            var termViewModels = new List<TermViewModel>();
            termViewModels.Add(new TermViewModel());
            var termFilter = new TermFilter();
            Guid userId = Guid.NewGuid();
            var paginationQuery = new PaginationQuery();
            this.authenticationHelper.Setup(h => h.GetLoggedInUser(this.termController.User)).Returns(userId);
            this.termService.Setup(s => s.SearchAsync(userId, termFilter, paginationQuery))
                .ReturnsAsync(termViewModels);

            IActionResult rs = await this.termController.SearchAsync(termFilter, paginationQuery);
            var objectResult = Assert.IsType<OkObjectResult>(rs);
            var termList = Assert.IsType<TermList>(objectResult.Value);
            Assert.Equal(termViewModels, termList.Items);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  dispose.
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.termController?.Dispose();
            }
        }
    }
}