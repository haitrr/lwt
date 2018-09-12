using System;
using System.Threading.Tasks;
using AutoMapper;
using Lwt.Interfaces.Services;
using LWT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Lwt.Interfaces;
using Lwt.Models;
using Lwt.ViewModels;

namespace Lwt.Controllers
{
    [Produces("application/json")]
    [Route("api/text")]
    public class TextController : Controller
    {
        private readonly ITextService _textService;
        private readonly IMapper _mapper;
        private readonly IAuthenticationHelper _authenticationHelper;

        public TextController(ITextService textService, IMapper mapper, IAuthenticationHelper authenticationHelper)
        {
            _textService = textService;
            _mapper = mapper;
            _authenticationHelper = authenticationHelper;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] TextCreateModel model)
        {
            Guid userId = _authenticationHelper.GetLoggedInUser(User);
            var text = _mapper.Map<Text>(model);
            await _textService.CreateAsync(userId, text);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAsync()
        {
            Guid userId = _authenticationHelper.GetLoggedInUser(User);
            IEnumerable<Text> texts = await _textService.GetByUserAsync(userId);
            var viewModels = _mapper.Map<IEnumerable<TextViewModel>>(texts);
            return Ok(viewModels);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            Guid userId = _authenticationHelper.GetLoggedInUser(User);
            await _textService.DeleteAsync(id, userId);
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditAsync([FromRoute] Guid id, [FromBody] TextEditModel editModel)
        {
            Guid userId = _authenticationHelper.GetLoggedInUser(User);
            await _textService.EditAsync(id, userId, editModel);
            return Ok();
        }
    }
}