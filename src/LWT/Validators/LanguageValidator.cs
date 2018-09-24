namespace Lwt.Validators
{
    using FluentValidation;
    using Lwt.Models;

#pragma warning disable CA1710 // Identifiers should have correct suffix
    /// <summary>
    /// a.
    /// </summary>
    public class LanguageValidator : AbstractValidator<Language>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageValidator"/> class.
        /// </summary>
        public LanguageValidator()
        {
            this.RuleFor(language => language.Name).NotEmpty().MaximumLength(30);
            this.RuleFor(language => language.CreatorId).NotEmpty();
        }
    }
}