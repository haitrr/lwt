namespace Lwt.Test.Utilities
{
    using System;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Utilities;
    using Moq;
    using Xunit;

    /// <summary>
    /// test the text separator.
    /// </summary>
    public class TextSeparatorTest
    {
        private readonly TextSeparator textSeparator;
        private readonly Mock<ILanguageHelper> languageHelperMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextSeparatorTest"/> class.
        /// </summary>
        public TextSeparatorTest()
        {
            this.languageHelperMock = new Mock<ILanguageHelper>();
            this.textSeparator = new TextSeparator(this.languageHelperMock.Object);
        }

        /// <summary>
        /// dependency injection should work.
        /// </summary>
        [Fact]
        public void ShouldGetSolved()
        {
            var helper = new DependencyResolverHelper();
            Assert.IsType<TextSeparator>(helper.GetService<ITextSeparator>());
        }

        /// <summary>
        /// if the language is not supported throw exception.
        /// </summary>
        [Fact]
        public void SeparateShouldThrowExceptionIfLanguageNotSupported()
        {
            var languageCode = LanguageCode.CHINESE;
            string text = string.Empty;
            this.languageHelperMock.Setup(h => h.GetLanguage(languageCode)).Throws<NotSupportedException>();
            Assert.Throws<NotSupportedException>(() => this.textSeparator.SeparateText(text, languageCode));
        }
    }
}