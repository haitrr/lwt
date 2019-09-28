namespace Lwt.Test.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Utilities;
    using Moq;
    using Xunit;

    /// <summary>
    /// TermCounter tester.
    /// </summary>
    public class TermCounterTester
    {
        private readonly TermCounter termCounter;
        private readonly Mock<ITermRepository> termRepositoryMock;
        private readonly Mock<ISkippedWordRemover> skippedWordRemoverMock;
        private readonly Mock<ITextNormalizer> textNormalizerMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermCounterTester"/> class.
        /// </summary>
        public TermCounterTester()
        {
            this.termRepositoryMock = new Mock<ITermRepository>();
            this.skippedWordRemoverMock = new Mock<ISkippedWordRemover>();
            this.textNormalizerMock = new Mock<ITextNormalizer>();
            this.termCounter = new TermCounter(
                this.termRepositoryMock.Object,
                this.skippedWordRemoverMock.Object,
                this.textNormalizerMock.Object);
        }

        /// <summary>
        /// should get resolved by DI.
        /// </summary>
        [Fact]
        public void ShouldGetResolveByDi()
        {
            var helper = new DependencyResolverHelper();
            Assert.IsType<TermCounter>(helper.GetService<ITermCounter>());
        }

        /// <summary>
        /// count by learning level should work as expected.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CountByLearningLevelAsyncShouldWork()
        {
            var words = new List<string> { "hello", "yolo", "hello", "kk" };
            var language = Language.English;
            Guid userId = Guid.NewGuid();
            var learningLevelDict = new Dictionary<string, TermLearningLevel>()
            {
                { "hello", TermLearningLevel.Learning1 },
                { "yolo", TermLearningLevel.Learning3 },
            };

            this.skippedWordRemoverMock.Setup(t => t.RemoveSkippedWords(words, language)).Returns(words);
            this.textNormalizerMock.Setup(n => n.Normalize(words, language)).Returns(words);
            this.termRepositoryMock.Setup(r => r.GetLearningLevelAsync(userId, language, It.IsAny<HashSet<string>>()))
                .ReturnsAsync(learningLevelDict);

            Dictionary<TermLearningLevel, long> result = await this.termCounter.CountByLearningLevelAsync(words, language, userId);
            Assert.Equal(Enum.GetValues(typeof(TermLearningLevel)).Length, result.Count);
            Assert.Equal(2, result[TermLearningLevel.Learning1]);
            Assert.Equal(1, result[TermLearningLevel.Learning3]);
            Assert.Equal(0, result[TermLearningLevel.Learning2]);
            Assert.Equal(0, result[TermLearningLevel.WellKnow]);
            Assert.Equal(1, result[TermLearningLevel.UnKnow]);
        }
    }
}