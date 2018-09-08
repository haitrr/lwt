using System;
using System.Threading.Tasks;
using AutoMapper;
using Lwt.Interfaces.Services;
using LWT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Lwt.Interfaces;
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
            Guid userId = _authenticationHelper.GetLoggedInUser(User.Identity);
            Text text = _mapper.Map<Text>(model);
            if (await _textService.CreateAsync(userId, text))
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAsync()
        {
            Guid userId = _authenticationHelper.GetLoggedInUser(User.Identity);
            IEnumerable<Text> texts = await _textService.GetByUserAsync(userId);
            IEnumerable<TextViewModel> viewModels = _mapper.Map<IEnumerable<TextViewModel>>(texts);
            return Ok(viewModels);
        }
    }
}