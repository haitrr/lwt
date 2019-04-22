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
        /// test count async.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CountAsyncShouldReturnCount()
        {
            Guid userId = Guid.NewGuid();
            long count = new Random().Next(1, 23132);
            var termFilter = new TermFilter();
            this.termRepository.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Term, bool>>>())).ReturnsAsync(count);

            long actual = await this.termService.CountAsync(userId, termFilter);

            Assert.Equal(count, actual);
        }

        /// <summary>
        /// test filter user id.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CountAsyncShouldFilterUserId()
        {
            Guid userId = Guid.NewGuid();
            long count = new Random().Next(1, 23132);
            var ownTerm = new Term { CreatorId = userId };
            var otherTerm = new Term { CreatorId = Guid.NewGuid() };
            var termFilter = new Mock<TermFilter>();
            termFilter.Setup(f => f.ToExpression()).Returns(term => true);
            Expression<Func<Term, bool>> actual = null;
            this.termRepository.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Term, bool>>>()))
                .Callback<Expression<Func<Term, bool>>>(filter => { actual = filter; }).ReturnsAsync(count);

            await this.termService.CountAsync(userId, termFilter.Object);

            Func<Term, bool> actualCompiled = actual.Compile();

            Assert.True(actualCompiled(ownTerm));
            Assert.False(actualCompiled(otherTerm));
        }

        /// <summary>
        /// test filter user id.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CountAsyncShouldUseFilter()
        {
            Guid userId = Guid.NewGuid();
            long count = new Random().Next(1, 23132);
            var ownTerm = new Term { CreatorId = userId };
            var termFilter = new Mock<TermFilter>();
            Expression<Func<Term, bool>> actual = null;
            this.termRepository.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Term, bool>>>()))
                .Callback<Expression<Func<Term, bool>>>(filter => { actual = filter; }).ReturnsAsync(count);
            termFilter.Setup(f => f.ToExpression()).Returns(term => true);

            await this.termService.CountAsync(userId, termFilter.Object);

            Func<Term, bool> actualCompiled = actual.Compile();
            Assert.True(actualCompiled(ownTerm));

            termFilter.Setup(f => f.ToExpression()).Returns(term => false);

            await this.termService.CountAsync(userId, termFilter.Object);

            actualCompiled = actual.Compile();
            Assert.False(actualCompiled(ownTerm));
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