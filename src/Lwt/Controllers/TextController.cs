namespace Lwt.Controllers
{
    using System;
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

        private readonly IMapper<TextCreateModel, Guid, Text> textCreateMapper;

        private readonly IMapper<Text, TextViewModel> textViewMapper;

        private readonly IAuthenticationHelper authenticationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextController"/> class.
        /// </summary>
        /// <param name="textService">textService.</param>
        /// <param name="textViewMapper">text view mapper.</param>
        /// <param name="authenticationHelper">authenticationHelper.</param>
        /// <param name="textCreateMapper">textCreateMapper.</param>
        public TextController(
            ITextService textService,
            IAuthenticationHelper authenticationHelper,
            IMapper<TextCreateModel, Guid, Text> textCreateMapper,
            IMapper<Text, TextViewModel> textViewMapper)
        {
            this.textService = textService;
            this.authenticationHelper = authenticationHelper;
            this.textCreateMapper = textCreateMapper;
            this.textViewMapper = textViewMapper;
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
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            Text text = this.textCreateMapper.Map(model, userId);
            await this.textService.CreateAsync(text);

            return this.Ok();
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
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            IEnumerable<Text> texts = await this.textService.GetByUserAsync(userId, filters, paginationQuery);
            int count = await this.textService.CountAsync(userId, filters);
            ICollection<TextViewModel> viewModels = this.textViewMapper.Map(texts);

            return this.Ok(new TextList { Total = count, Items = viewModels });
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="id">id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            await this.textService.DeleteAsync(id, userId);

            return this.Ok();
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="editModel">editModel.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditAsync([FromRoute] Guid id, [FromBody] TextEditModel editModel)
        {
            Guid userId = this.authenticationHelper.GetLoggedInUser(this.User);
            await this.textService.EditAsync(id, userId, editModel);

            return this.Ok();
        }
    }
}