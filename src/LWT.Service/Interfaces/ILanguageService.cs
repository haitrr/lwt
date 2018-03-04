using System.Threading.Tasks;
using LWT.Service.ViewModels;

namespace LWT.Service.Interfaces
{
    public interface ILanguageService : IService
    {
        Task<bool> Add(AddLanguageViewModel addLanguageViewModel);
    }
}