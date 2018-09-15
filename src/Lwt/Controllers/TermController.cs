using System;
using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lwt.Controllers
{
    [Route("/api/term")]
    public class TermController : Controller
    {
        private readonly ITermService _termService;
        private readonly IMapper<TermCreateModel, Guid, Term> _termCreateMapper;
        private readonly IAuthenticationHelper _authenticationHelper;

        public TermController(ITermService termService, IMapper<TermCreateModel, Guid, Term> termCreateMapper,
            IAuthenticationHelper authenticationHelper)
        {
            _termService = termService;
            _termCreateMapper = termCreateMapper;
            _authenticationHelper = authenticationHelper;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] TermCreateModel termCreateModel)
        {
            Guid userId = _authenticationHelper.GetLoggedInUser(User);
            Term term = _termCreateMapper.Map(termCreateModel, userId);
            Guid id = await _termService.CreateAsync(term);
            return Ok(id);
        }
    }
}