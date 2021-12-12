using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lwt.Clients;

public interface IJapaneseSegmenterClient
{
    Task<IEnumerable<string>> CutAsync(string text);
}