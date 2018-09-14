using FluentValidation;
using Lwt.Interfaces;
using Lwt.Models;
using LWT.Models;
using Microsoft.AspNetCore.Identity;

namespace Lwt.Validators
{
    public class TextValidator : AbstractValidator<Text>
    {
        public TextValidator(UserManager<User> userManager, ILanguageRepository languageRepository)
        {
            RuleFor(text => text.UserId).NotEmpty().MustAsync(async (id, token) =>
                await userManager.FindByIdAsync(id.ToString()) != null).WithMessage("Creator does not exist");
            RuleFor(text => text.Content).NotEmpty();
            RuleFor(text => text.Title).NotEmpty();

            RuleFor(text => text.LanguageId).NotEmpty()
                .MustAsync(async (id, toke) => await languageRepository.IsExists(id))
                .WithMessage("Text language is not exist.");
        }
    }
}