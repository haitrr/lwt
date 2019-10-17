namespace Lwt.Test.Utilities
{
    using System;
    using System.Collections.Generic;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Utilities;
    using Moq;
    using Xunit;

    /// <summary>
    /// test language helper.
    /// </summary>
    public class LanguageHelperTest
    {
        private readonly LanguageHelper languageHelper;
        private readonly Mock<IServiceProvider> serviceProviderMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageHelperTest"/> class.
        /// </summary>
        public LanguageHelperTest()
        {
            this.serviceProviderMock = new Mock<IServiceProvider>();
            this.languageHelper = new LanguageHelper(this.serviceProviderMock.Object);
        }

        /// <summary>
        /// can get english.
        /// </summary>
        [Fact]
        public void GetLanguageShouldReturnEnglish()
        {
            ILanguage actual = this.languageHelper.GetLanguage(Language.English);
            Assert.IsType<English>(actual);
        }

        /// <summary>
        /// can get chinese.
        /// </summary>
        [Fact]
        public void GetLanguageShouldReturnChinese()
        {
            ILanguage actual = this.languageHelper.GetLanguage(Language.Chinese);
            Assert.IsType<Chinese>(actual);
        }

        /// <summary>
        /// can get japanese.
        /// </summary>
        [Fact]
        public void GetLanguageShouldReturnJapanese()
        {
            ILanguage actual = this.languageHelper.GetLanguage(Language.Japanese);
            Assert.IsType<Japanese>(actual);
        }

        /// <summary>
        /// can get vietnamese.
        /// </summary>
        [Fact]
        public void GetLanguageShouldReturnVietnamese()
        {
            ILanguage actual = this.languageHelper.GetLanguage(Language.Vietnamese);
            Assert.IsType<Vietnamese>(actual);
        }

        /// <summary>
        /// throw if language not supported.
        /// </summary>
        [Fact]
        public void GetLanguageShouldThrowIfNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => this.languageHelper.GetLanguage((Language)342));
        }

        /// <summary>
        /// can get all languages.
        /// </summary>
        [Fact]
        public void GetAllLanguageShouldReturnAllLanguages()
        {
            ICollection<ILanguage> actual = this.languageHelper.GetAllLanguages();
            Assert.Contains(actual, l => l.Name == "Vietnamese");
            Assert.Contains(actual, l => l.Name == "English");
            Assert.Contains(actual, l => l.Name == "Japanese");
            Assert.Contains(actual, l => l.Name == "Chinese");
        }
    }
}