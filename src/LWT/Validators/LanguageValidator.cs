namespace Lwt.Validators
{
    using FluentValidation;

    using Lwt.Interfaces;
    using Lwt.Models;

#pragma warning disable CA1710 // Identifiers should have correct suffix
    /// <summary>
    /// Validator for language.
    /// </summary>
    public class LanguageValidator : AbstractValidator<Language>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageValidator"/> class.
        /// </summary>
        /// <param name="userRepository">user repository.</param>
        public LanguageValidator(IUserRepository userRepository)
        {
            this.RuleFor(language => language.Name).NotEmpty().MaximumLength(30);

            this.RuleFor(language => language.CreatorId).NotEmpty()
                .MustAsync(async (id, token) => await userRepository.IsExistAsync(id))
                .WithMessage("User does not exist.");
        }
    }
}