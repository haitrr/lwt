namespace Lwt.Models;

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

/// <summary>
/// Log object to store event logs.
/// </summary>
public record Log(string Event, JObject Data, string UserName) : Entity
{
    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    [Required]
    public string Event { get; set; } = Event;

    /// <summary>
    /// Gets or sets the event data.
    /// </summary>
    [Required]
    public JObject Data { get; set; } = Data;

    /// <summary>
    /// Gets or sets the user id that this log belong to.
    /// </summary>
    [Required]
    public string UserName { get; set; } = UserName;
}