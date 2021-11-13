namespace Lwt.Services;

using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Models;

/// <inheritdoc />
public class UserPasswordChanger : IUserPasswordChanger
{
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPasswordChanger"/> class.
    /// </summary>
    /// <param name="userRepository">the user repository.</param>
    public UserPasswordChanger(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        User user = await this.userRepository.GetByIdAsync(userId);

        return await this.userRepository.ChangePasswordAsync(user, currentPassword, newPassword);
    }
}