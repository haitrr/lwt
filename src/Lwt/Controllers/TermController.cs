namespace Lwt.Controllers;

using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.ViewModels;
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

    private readonly IMapper<TermCreateModel, int, Term> termCreateMapper;

    private readonly IAuthenticationHelper authenticationHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TermController"/> class.
    /// </summary>
    /// <param name="termService">the term service.</param>
    /// <param name="termCreateMapper">the term create mapper.</param>
    /// <param name="authenticationHelper">the authentication helper.</param>
    public TermController(
        ITermService termService,
        IMapper<TermCreateModel, int, Term> termCreateMapper,
        IAuthenticationHelper authenticationHelper)
    {
        this.termService = termService;
        this.termCreateMapper = termCreateMapper;
        this.authenticationHelper = authenticationHelper;
    }

    /// <summary>
    /// create a term.
    /// </summary>
    /// <param name="termCreateModel">create model.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync([FromBody] TermCreateModel termCreateModel)
    {
        int userId = this.authenticationHelper.GetLoggedInUser();
        Term term = this.termCreateMapper.Map(termCreateModel, userId);
        int id = await this.termService.CreateAsync(term);

        return this.Ok(new CreateTermResponse { Id = id });
    }

    /// <summary>
    /// get a term.
    /// </summary>
    /// <param name="termId">the term's id.</param>
    /// <returns>the term view model.</returns>
    [HttpGet("{termId}")]
    [Authorize]
    public async Task<IActionResult> GetAsync([FromRoute] int termId)
    {
        int userId = this.authenticationHelper.GetLoggedInUser();
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
    public async Task<IActionResult> EditAsync([FromBody] TermEditModel termEditModel, [FromRoute] int id)
    {
        int userId = this.authenticationHelper.GetLoggedInUser();
        await this.termService.EditAsync(termEditModel, id, userId);

        return this.Ok(new { });
    }

    /// <summary>
    /// get a term.
    /// </summary>
    /// <param name="termId">the term's id.</param>
    /// <returns>the term meaning.</returns>
    [ETagFilter(200)]
    [Authorize]
    [HttpGet("{termId}/meaning")]
    public async Task<IActionResult> GetMeaningAsync([FromRoute] int termId)
    {
        int userId = this.authenticationHelper.GetLoggedInUser();
        TermMeaningDto termMeaningDto = await this.termService.GetMeaningAsync(userId, termId);
        return this.Ok(termMeaningDto);
    }
}