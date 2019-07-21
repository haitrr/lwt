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
        private Mock<ITermRepository> termRepository;
        private Mock<IMapper<TermEditModel, Term>> termEditMapper;
        private Mock<IMapper<Term, TermViewModel>> termViewMapper;
        private TermService termService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermServiceTest"/> class.
        /// </summary>
        public TermServiceTest()
        {
            this.termEditMapper = new Mock<IMapper<TermEditModel, Term>>();
            this.termViewMapper = new Mock<IMapper<Term, TermViewModel>>();
            this.termRepository = new Mock<ITermRepository>();
            this.termService = new TermService(
                this.termRepository.Object,
                this.termEditMapper.Object,
                this.termViewMapper.Object);
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
            this.termRepository.Setup(
                    r => r.SearchAsync(It.IsAny<Expression<Func<Term, bool>>>(), It.IsAny<PaginationQuery>()))
                .ReturnsAsync(terms);
            this.termViewMapper.Setup(m => m.Map(terms)).Returns(termViewModels);

            IEnumerable<TermViewModel> actual = await this.termService.SearchAsync(
                It.IsAny<Guid>(),
                termFilter,
                It.IsAny<PaginationQuery>());

            Assert.Equal(termViewModels, actual);
        }
    }
}