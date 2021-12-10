using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Services;

public interface ITextReader
{
    Task<TextReadModel> ReadAsync(int id, int userId);
    Task<Text?> GetLastReadAsync(string userName);
}