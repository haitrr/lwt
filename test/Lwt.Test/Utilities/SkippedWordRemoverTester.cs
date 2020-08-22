namespace Lwt.Test.Utilities
{
    using System.Collections.Generic;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Utilities;
    using Moq;
    using Xunit;

    /// <summary>
    /// skipped word remover tester.
    /// </summary>
    public class SkippedWordRemoverTester
    {
        private readonly SkippedWordRemover skippedWordRemover;
        private readonly Mock<ILanguageHelper> languageHelperMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkippedWordRemoverTester"/> class.
        /// </summary>
        public SkippedWordRemoverTester()
        {
            this.languageHelperMock = new Mock<ILanguageHelper>();
            this.skippedWordRemover = new SkippedWordRemover(this.languageHelperMock.Object);
        }

        /// <summary>
        /// should get resolved by DI.
        /// </summary>
        [Fact]
        public void ShouldGetResolveByDi()
        {
            var helper = new DependencyResolverHelper();
            Assert.IsType<SkippedWordRemover>(helper.GetService<ISkippedWordRemover>());
        }

        /// <summary>
        /// test if the remover removes the right words.
        /// </summary>
        [Fact]
        public void SkippedWordRemoverShouldRemoveSkippedWord()
        {
            var words = new List<string> { "sdf", "dsfsd", "dfjk", "sdf" };
            var languageMock = new Mock<ILanguage>();
            languageMock.Setup(l => l.ShouldSkip("sdf")).Returns(true);
            this.languageHelperMock.Setup(h => h.GetLanguage(It.IsAny<LanguageCode>()))
                .Returns(languageMock.Object);

            IEnumerable<string> result = this.skippedWordRemover.RemoveSkippedWords(words, LanguageCode.ENGLISH);

            Assert.DoesNotContain("sdf", result);
            Assert.Contains("dsfsd", result);
            Assert.Contains("dfjk", result);
        }
    }
}