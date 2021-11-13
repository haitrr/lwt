namespace Lwt.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// a.
/// </summary>
public record Text : Entity
{
    /// <summary>
    /// table name.
    /// </summary>
    public const string TableName = "texts";

    /// <summary>
    /// Gets or sets creator id.
    /// </summary>
    public int UserId { get; set; }

    public User? User { get; set; }

    /// <summary>
    /// Gets or sets bookmark by user.
    /// </summary>
    public ulong? Bookmark { get; set; }

    /// <summary>
    /// Gets or sets Title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets language.
    /// </summary>
    public LanguageCode LanguageCode { get; set; } = null!;

    public List<TextTerm> TextTerms { get; set; } = null!;

    public long TermCount { get; set; }

    public long ProcessedTermCount { get; set; }

    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;
}