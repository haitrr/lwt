using LWT.Service.ViewModels;
using System.Threading.Tasks;

namespace LWT.Service.Interfaces
{
    public interface ILanguageService : IService
    {
        public Task<bool> Add(AddLanguageViewModel addLanguageViewModel);
    }
}