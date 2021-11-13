namespace Lwt.Models;

using Newtonsoft.Json;

/// <summary>
/// a.
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDetails"/> class.
    /// </summary>
    /// <param name="message">message.</param>
    public ErrorDetails(string message)
    {
        this.Message = message;
    }

    /// <summary>
    /// Gets a.
    /// </summary>
    public string Message { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}