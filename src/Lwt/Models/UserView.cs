namespace Lwt.Models
{
  /// <summary>
  /// user view model.
  /// </summary>
  public class UserView
  {
    /// <summary>
    /// Gets or sets user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets user email.
    /// </summary>
    public string Email { get; set; } = string.Empty;
  }
}