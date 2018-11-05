namespace Lwt.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Lwt.Interfaces;
    using Lwt.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <inheritdoc />
    /// <summary>
    /// a.
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
        /// <param name="termService">a.</param>
        /// <param name="termCreateMapper">aaa.</param>
        /// <param name="authenticationHelper">aa.</param>
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
    }
}