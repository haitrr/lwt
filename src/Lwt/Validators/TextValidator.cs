namespace Lwt.Validators;

using FluentValidation;

using Lwt.Models;

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
    public TextValidator(UserManager<User> userManager)
    {
        this.RuleFor(text => text.UserId).NotEmpty()
            .MustAsync(async (id, _) => await userManager.FindByIdAsync(id.ToString()) != null)
            .WithMessage("Creator does not exist");

        this.RuleFor(text => text.Content).NotEmpty();
        this.RuleFor(text => text.Title).NotEmpty();
    }
}