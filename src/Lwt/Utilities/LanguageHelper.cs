using Lwt.Clients;

namespace Lwt.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using Lwt.Interfaces;
using Lwt.Models;
using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public class LanguageHelper : ILanguageHelper
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageHelper"/> class.
    /// </summary>
    /// <param name="serviceProvider">the service provider.</param>
    public LanguageHelper(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public ILanguage GetLanguage(LanguageCode languageCode)
    {
        var languages = new ILanguage[]
        {
            new Vietnamese(), new English(),
            new Chinese(this.serviceProvider.GetService<IChineseTextSplitter>() !),
            new Japanese(this.serviceProvider.GetService<IJapaneseSegmenterClient>() !),
        };

        ILanguage? language = languages.SingleOrDefault(l => l.Code == languageCode);

        if (language != null)
        {
            return language;
        }

        throw new NotSupportedException($"Language {languageCode} is not supported.");
    }

    /// <inheritdoc/>
    public ICollection<ILanguage> GetAllLanguages()
    {
        return new List<ILanguage>
        {
            new English(),
            new Chinese(this.serviceProvider.GetService<IChineseTextSplitter>() !),
            new Japanese(this.serviceProvider.GetService<IJapaneseSegmenterClient>() !),
            new Vietnamese(),
        };
    }
}