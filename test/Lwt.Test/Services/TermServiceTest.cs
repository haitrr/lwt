namespace Lwt.Test.Services
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Services;
    using Lwt.ViewModels;
    using Moq;
    using Xunit;

    /// <summary>
    /// Term service test.
    /// </summary>
    public class TermServiceTest
    {
        private Mock<ISqlTermRepository> termRepositoryMock;
        private Mock<IMapper<TermEditModel, Term>> termEditMapperMock;
        private Mock<IMapper<Term, TermViewModel>> termViewMapperMock;
        private Mock<IMapper<Term, TermMeaningDto>> termMeaningMapper;
        private TermService termService;
        private Mock<IDbTransaction> dbTransaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermServiceTest"/> class.
        /// </summary>
        public TermServiceTest()
        {
            this.termEditMapperMock = new Mock<IMapper<TermEditModel, Term>>();
            this.termViewMapperMock = new Mock<IMapper<Term, TermViewModel>>();
            this.termRepositoryMock = new Mock<ISqlTermRepository>();
            this.termMeaningMapper = new Mock<IMapper<Term, TermMeaningDto>>();
            this.dbTransaction = new Mock<IDbTransaction>();
            this.termService = new TermService(
                this.termRepositoryMock.Object,
                this.termEditMapperMock.Object,
                this.termViewMapperMock.Object,
                this.termMeaningMapper.Object,
                this.dbTransaction.Object);
        }

        /// <summary>
        /// create async should return the new term id.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsyncShouldReturnTermId()
        {
            var term = new Term();

            int result = await this.termService.CreateAsync(term);

            Assert.Equal(term.Id, result);
        }

        /// <summary>
        /// term edit should update mapped term.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task EditAsyncShouldUpdateMappedTerm()
        {
            var userId = 1;
            var termId = 1;
            var termEdit = new TermEditModel();
            var current = new Term();
            this.termRepositoryMock.Setup(r => r.GetUserTermAsync(termId, userId))
                .ReturnsAsync(current);
            this.termEditMapperMock.Setup(m => m.Map(termEdit, current))
                .Returns(current);

            await this.termService.EditAsync(termEdit, termId, userId);
            this.termRepositoryMock.Verify(r => r.Update(current));
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
        public async Task CountAsyncShouldReturnCountFromRepository(int count)
        {
            this.termRepositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Term, bool>>>()))
                .ReturnsAsync(count);

            long rs = await this.termService.CountAsync(1, new TermFilter());
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
            var userId = 1;
            var termId = 1;
            this.termRepositoryMock.Setup(r => r.GetUserTermAsync(termId, userId))
                .ReturnsAsync(term);
            this.termViewMapperMock.Setup(m => m.Map(term))
                .Returns(termViewModel);

            TermViewModel result = await this.termService.GetAsync(userId, termId);

            Assert.Equal(termViewModel, result);
        }
    }
}