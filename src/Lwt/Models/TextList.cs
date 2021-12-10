namespace Lwt.Models;

using System.Collections.Generic;

using Lwt.ViewModels;

/// <summary>
/// the text list with pagination.
/// </summary>
public class TextList
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextList"/> class.
    /// </summary>
    /// <param name="total">total texts.</param>
    /// <param name="items">texts.</param>
    public TextList(long total, IEnumerable<TextViewModel?> items)
    {
        this.Total = total;
        this.Items = items;
    }

    /// <summary>
    /// Gets or sets the total number of texts.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Gets or sets the text in the current page.
    /// </summary>
    public IEnumerable<TextViewModel?> Items { get; set; }
}