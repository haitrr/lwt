using System.ComponentModel.DataAnnotations;

namespace LWT.Service.ViewModels
{
    public class AddLanguageViewModel
    {
        public AddLanguageViewModel(string name)
        {
            Name = name;
        }

        [Required] public string Name { get; set; }
    }
}