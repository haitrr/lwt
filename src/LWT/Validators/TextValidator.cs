namespace Lwt.Validators
{
    using FluentValidation;

    using Lwt.Interfaces;
    using Lwt.Models;

    using LWT.Models;

    using Microsoft.AspNetCore.Identity;

#pragma warning disable CA1710 // Identifiers should have correct suffix
    /// <summary>
    /// a.
    /// </summary>
    public class TextValidator : AbstractValidator<Text>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextValidator"/> class.
        /// </summary>
        /// <param name="userManager">userManager.</param>
        /// <param name="languageRepository">languageRepository.</param>
        public TextValidator(UserManager<User> userManager, ILanguageRepository languageRepository)
        {
            this.RuleFor(text => text.UserId).NotEmpty()
                .MustAsync(async (id, token) => await userManager.FindByIdAsync(id.ToString()) != null)
                .WithMessage("Creator does not exist");

            this.RuleFor(text => text.Content).NotEmpty();
            this.RuleFor(text => text.Title).NotEmpty();

            this.RuleFor(text => text.LanguageId).NotEmpty()
                .MustAsync(async (id, toke) => await languageRepository.IsExists(id))
                .WithMessage("Text language is not exist.");
        }
    }
}