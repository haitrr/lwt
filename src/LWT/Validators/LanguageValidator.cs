using FluentValidation;
using Lwt.Models;

namespace Lwt.Validators
{
    public class LanguageValidator : AbstractValidator<Language>
    {
        public LanguageValidator()
        {
            RuleFor(language => language.Name).NotEmpty().MaximumLength(30);
            RuleFor(language => language.CreatorId).NotEmpty();
        }
    }
}