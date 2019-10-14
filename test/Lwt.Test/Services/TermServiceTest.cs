namespace Lwt.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// Term service test.
    /// </summary>
    public class TermServiceTest
    {
        private Mock<ITermRepository> termRepositoryMock;
        private Mock<IMapper<TermEditModel, Term>> termEditMapperMock;
        private Mock<IMapper<Term, TermViewModel>> termViewMapperMock;
        private TermService termService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermServiceTest"/> class.
        /// </summary>
        public TermServiceTest()
        {
            this.termEditMapperMock = new Mock<IMapper<TermEditModel, Term>>();
            this.termViewMapperMock = new Mock<IMapper<Term, TermViewModel>>();
            this.termRepositoryMock = new Mock<ITermRepository>();
            this.termService = new TermService(
                this.termRepositoryMock.Object,
                this.termEditMapperMock.Object,
                this.termViewMapperMock.Object);
        }

        /// <summary>
        /// test search async.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SearchAsyncShouldReturnListResults()
        {
            var terms = new List<Term>();
            var termViewModels = new List<TermViewModel>();
            var termFilter = new TermFilter();
            this.termRepositoryMock.Setup(
                    r => r.SearchAsync(It.IsAny<Expression<Func<Term, bool>>>(), It.IsAny<PaginationQuery>()))
                .ReturnsAsync(terms);
            this.termViewMapperMock.Setup(m => m.Map(terms))
                .Returns(termViewModels);

            IEnumerable<TermViewModel> actual = await this.termService.SearchAsync(
                It.IsAny<Guid>(),
                termFilter,
                It.IsAny<PaginationQuery>());

            Assert.Equal(termViewModels, actual);
        }

        /// <summary>
        /// create async should return the new term id.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsyncShouldReturnTermId()
        {
            var term = new Term();

            Guid result = await this.termService.CreateAsync(term);

            Assert.Equal(term.Id, result);
        }

        /// <summary>
        /// term edit should update mapped term.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EditAsyncShouldUpdateMappedTerm()
        {
            Guid userId = Guid.NewGuid();
            Guid termId = Guid.NewGuid();
            var termEdit = new TermEditModel();
            var current = new Term();
            this.termRepositoryMock.Setup(r => r.GetUserTermAsync(termId, userId))
                .ReturnsAsync(current);
            this.termEditMapperMock.Setup(m => m.Map(termEdit, current))
                .Returns(current);

            await this.termService.EditAsync(termEdit, termId, userId);
            this.termRepositoryMock.Verify(r => r.UpdateAsync(current));
        }

        /// <summary>
        /// count async should return count from repository.
        /// </summary>
        /// <param name="count">the count.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(42)]
        [InlineData(4432)]
        [InlineData(2)]
        [InlineData(6)]
        public async Task CountAsyncShouldReturnCountFromRepository(long count)
        {
            this.termRepositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Term, bool>>>()))
                .ReturnsAsync(count);

            long rs = await this.termService.CountAsync(Guid.NewGuid(), new TermFilter());
            Assert.Equal(count, rs);
        }

        /// <summary>
        /// get async should return mapped result.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsyncShouldReturnMappedTerm()
        {
            var term = new Term();
            var termViewModel = new TermViewModel();
            Guid userId = Guid.NewGuid();
            Guid termId = Guid.NewGuid();
            this.termRepositoryMock.Setup(r => r.GetUserTermAsync(termId, userId))
                .ReturnsAsync(term);
            this.termViewMapperMock.Setup(m => m.Map(term))
                .Returns(termViewModel);

            TermViewModel result = await this.termService.GetAsync(userId, termId);

            Assert.Equal(termViewModel, result);
        }
    }
}