namespace Lwt.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;
    using Lwt.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <inheritdoc />
    /// <summary>
    /// a.
    /// </summary>
    [Produces("application/json")]
    [Route("api/text")]
    public class TextController : Controller
    {
        private readonly ITextService textService;

        private readonly IMapper<TextCreateModel, int, Text> textCreateMapper;

        private readonly IAuthenticationHelper authenticationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextController"/> class.
        /// </summary>
        /// <param name="textService">textService.</param>
        /// <param name="authenticationHelper">authenticationHelper.</param>
        /// <param name="textCreateMapper">textCreateMapper.</param>
        public TextController(
            ITextService textService,
            IAuthenticationHelper authenticationHelper,
            IMapper<TextCreateModel, int, Text> textCreateMapper)
        {
            this.textService = textService;
            this.authenticationHelper = authenticationHelper;
            this.textCreateMapper = textCreateMapper;
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="model">model.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] TextCreateModel model)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);
            Text text = this.textCreateMapper.Map(model, userId);
            int id = await this.textService.CreateAsync(text);

            return this.Ok(new { Id = id });
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="filters"> the filters.</param>
        /// <param name="paginationQuery"> the pagination query.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] TextFilter filters,
            [FromQuery] PaginationQuery paginationQuery)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);

            long count = await this.textService.CountAsync(userId, filters);
            IEnumerable<TextViewModel> viewModels =
                await this.textService.GetByUserAsync(userId, filters, paginationQuery);

            return this.Ok(new TextList(count, viewModels));
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);
            await this.textService.DeleteAsync(id, userId);

            return this.Ok();
        }

        /// <summary>
        /// get the text for reading.
        /// </summary>
        /// <param name="id">the text id.</param>
        /// <returns>the text read model.</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> ReadAsync([FromRoute] int id)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);
            TextReadModel textReadModel = await this.textService.ReadAsync(id, userId);
            textReadModel.TermCount = await this.textService.CountTextTermsAsync(id, userId);

            return this.Ok(textReadModel);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="editModel">editModel.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditAsync([FromRoute] int id, [FromBody] TextEditModel editModel)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);
            await this.textService.EditAsync(id, userId, editModel);

            return this.Ok();
        }

        /// <summary>
        /// get text detail for edit.
        /// </summary>
        /// <param name="id">the text id.</param>
        /// <returns>edit details.</returns>
        [HttpGet("{id}/edit-details")]
        [Authorize]
        public async Task<IActionResult> GetEditDetailAsync([FromRoute] int id)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);
            TextEditDetailModel editDetail = await this.textService.GetEditDetailAsync(id, userId);

            return this.Ok(editDetail);
        }

        /// <summary>
        /// set text bookmark.
        /// </summary>
        /// <param name="id">text id.</param>
        /// <param name="setBookmarkModel">set bookmark model.</param>
        /// <returns>Status.</returns>
        [HttpPatch("{id}/bookmark")]
        [Authorize]
        public async Task<IActionResult> SetBookmarkAsync(
            [FromRoute] int id,
            [FromBody] SetBookmarkModel setBookmarkModel)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);
            await this.textService.SetBookmarkAsync(id, userId, setBookmarkModel);
            return this.Ok();
        }

        [HttpGet("{id}/term-counts")]
        [Authorize]
        public async Task<IActionResult> GetTermCounts([FromRoute] int id)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);
            Dictionary<LearningLevel, int> counts = await this.textService.GetTermCountsAsync(id, userId);
            return this.Ok(new { Id = id, Counts = counts });
        }

        [HttpGet("{id}/terms")]
        [Authorize]
        public async Task<IActionResult> GetTerms([FromRoute] int id, [FromQuery] int indexFrom, [FromQuery]int indexTo)
        {
            int userId = this.authenticationHelper.GetLoggedInUser(this.User);
            IEnumerable<TermReadModel> terms = await this.textService.GetTextTermsAsync(id, userId, indexFrom, indexTo);
            int totalCount = await this.textService.CountTextTermsAsync(id, userId);
            return this.Ok(new { Id = id, Terms = terms, TotalCount = totalCount });
        }
    }
}