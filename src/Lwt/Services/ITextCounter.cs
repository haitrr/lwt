using System.Collections.Generic;
using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Services;

public interface ITextCounter
{
    Task<List<CountByLanguageCode>> CountByLanguageAsync(int userId, TextFilter filters);
}