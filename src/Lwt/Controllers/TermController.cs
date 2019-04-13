namespace Lwt.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <inheritdoc />
    /// <summary>
    /// the controller for term apis.
    /// </summary>
    [Route("/api/term")]
    public class TermController : Controller
    {
        private readonly ITermService termService;

        private readonly IMapper<TermCreateModel, Guid, Term> termCreateMapper;

        private readonly IAuthenticationHelper authenticationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermController"/> class.
        /// </summary>
        /// <param name="termService">the term service.</param>
        /// <param name="termCreateMapper">the term create mapper.</param>
        /// <param name="authenticationHelper">the authentication helper.</param>
        public TermController(
            ITermService termService,
            IMapper<TermCreateModel, Guid, Term> termCreateMapper,
            IAuthenticationHelper authenticationHelper)
        {
            this.termService = termService;
            this.termCreateMapper = termCreateMapper;
            this.authenticationHelper = authenticationHelper;
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="termCreateModel">asdasd.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] TermCreateModel termCreateModel)
        {
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            Term term = this.termCreateMapper.Map(termCreateModel, userId);
            Guid id = await this.termService.CreateAsync(term);

            return this.Ok(id);
        }

        /// <summary>
        /// get a term.
        /// </summary>
        /// <param name="termId">the term's id.</param>
        /// <returns>the term view model.</returns>
        [HttpGet("{termId}")]
        [Authorize]
        public async Task<IActionResult> GetAsync([FromRoute] Guid termId)
        {
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            TermViewModel termViewModel = await this.termService.GetAsync(userId, termId);
            return this.Ok(termViewModel);
        }

        /// <summary>
        /// edit a term.
        /// </summary>
        /// <param name="termEditModel">the edit model.</param>
        /// <param name="id">the term id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] TermEditModel termEditModel, [FromRoute] Guid id)
        {
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            await this.termService.EditAsync(termEditModel, id, userId);

            return this.Ok(new { });
        }

        /// <summary>
        /// get all terms match filter.
        /// </summary>
        /// <param name="termFilter">the filter.</param>
        /// <param name="paginationQuery">the pagination query.</param>
        /// <returns>the terms list.</returns>
        [Authorize]
        public async Task<IActionResult> SearchAsync(TermFilter termFilter, PaginationQuery paginationQuery)
        {
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            ulong total = await this.termService.CountAsync(userId, termFilter);
            IEnumerable<TermViewModel> termViewModels =
                await this.termService.SearchAsync(userId, termFilter, paginationQuery);
            return this.Ok(new TermList { Total = total, Items = termViewModels });
        }
    }
}