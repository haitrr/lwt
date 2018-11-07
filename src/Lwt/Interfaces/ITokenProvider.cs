namespace Lwt.Interfaces
{
    using Lwt.Models;

    /// <summary>
    /// Token provider.
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Generate a new jwt authentication token of a user.
        /// </summary>
        /// <param name="user">the user.</param>
        /// <returns>the jwt authentication token.</returns>
        string GenerateUserToken(User user);
    }
}