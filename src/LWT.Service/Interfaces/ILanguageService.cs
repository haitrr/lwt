using LWT.Service.ViewModels;
using System.Threading.Tasks;

namespace LWT.Service.Interfaces
{
    public interface ILanguageService : IService
    {
        Task<bool> Add(AddLanguageViewModel addLanguageViewModel);
    }
}