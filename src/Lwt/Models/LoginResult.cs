namespace Lwt.Models
{
    /// <summary>
    /// login result object.
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// Gets or sets authentication token.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}