namespace Lwt.Models
{
  using System.Collections.Generic;

  /// <summary>
  /// term list.
  /// </summary>
  public class TermList
  {
    /// <summary>
    /// Gets or sets total results.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Gets or sets items list.
    /// </summary>
    public IEnumerable<TermViewModel> Items { get; set; } = new TermViewModel[] { };
  }
}