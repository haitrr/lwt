namespace Lwt.Models
{
    /// <summary>
    /// change password model.
    /// </summary>
    public class UserChangePasswordModel
    {
        /// <summary>
        /// Gets or sets current password.
        /// </summary>
        public string CurrentPassword { get; set; }

        /// <summary>
        /// Gets or sets new password.
        /// </summary>
        public string NewPassword { get; set; }
    }
}