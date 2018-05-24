using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lwt.Controllers
{
    [Produces("application/json")]
    [Route("api/text")]
    public class TextController : Controller
    {
        private readonly ITextService _textService;

        public TextController(ITextService textService)
        {
            _textService = textService;
        }

        public async Task CreateAsync()
        {
            await _textService.CreateAsync();
        }
    }
}