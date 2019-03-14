namespace Lwt.Utilities
{
    using System;
    using System.Collections.Generic;
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
        public ILanguage GetLanguage(Language language)
        {
            switch (language)
            {
                case Language.English:

                    return new English();
                case Language.Chinese:
                    return new Chinese(this.serviceProvider.GetService<IChineseTextSplitter>());
                case Language.Japanese:
                    return new Japanese(this.serviceProvider.GetService<JapaneseTextSplitter>());
            }

            throw new NotSupportedException($"Language {language.ToString()} is not supported.");
        }

        /// <inheritdoc/>
        public ICollection<ILanguage> GetAllLanguages()
        {
            return new List<ILanguage>()
            {
                new English(),
                new Chinese(this.serviceProvider.GetService<IChineseTextSplitter>()),
                new Japanese(this.serviceProvider.GetService<JapaneseTextSplitter>()),
            };
        }
    }
}