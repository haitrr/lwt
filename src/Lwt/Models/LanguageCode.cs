namespace Lwt.Models;

using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

/// <summary>
/// language code.
/// </summary>
[JsonConverter(typeof(LanguageCodeJsonConverter))]
[ModelBinder(BinderType = typeof(LanguageCodeModelBinder))]
public sealed class LanguageCode
{
    /// <summary>
    /// Chinese.
    /// </summary>
    public static readonly LanguageCode CHINESE = new LanguageCode("zh");

    /// <summary>
    /// English.
    /// </summary>
    public static readonly LanguageCode ENGLISH = new LanguageCode("en");

    /// <summary>
    /// Japanese.
    /// </summary>
    public static readonly LanguageCode JAPANESE = new LanguageCode("ja");

    /// <summary>
    /// Vietnamese.
    /// </summary>
    public static readonly LanguageCode VIETNAMESE = new LanguageCode("vi");

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageCode"/> class.
    /// </summary>
    /// <param name="value"> value.</param>
    private LanguageCode(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets value of code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// get language code from string.
    /// </summary>
    /// <param name="code">code.</param>
    /// <returns>language code.</returns>
    /// <exception cref="NotSupportedException">language code is not supported.</exception>
    public static LanguageCode GetFromString(string code)
    {
        switch (code)
        {
            case "en":
                return ENGLISH;
            case "zh":
                return CHINESE;
            case "vi":
                return VIETNAMESE;
            case "ja":
                return JAPANESE;
            default:
                throw new NotSupportedException();
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.Value;
    }
}