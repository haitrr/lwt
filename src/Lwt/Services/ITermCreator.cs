using System.Threading.Tasks;
using Lwt.Models;

namespace Lwt.Services;

public interface ITermCreator
{
    Task<int> CreateAsync(Term term);
}