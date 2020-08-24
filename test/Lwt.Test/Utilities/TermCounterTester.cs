namespace Lwt.Test.Utilities
{
    using System.Collections.Generic;
    using System.Linq;
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
        private readonly Mock<ISqlTermRepository> termRepositoryMock;
        private readonly Mock<ISkippedWordRemover> skippedWordRemoverMock;
        private readonly Mock<ITextNormalizer> textNormalizerMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermCounterTester"/> class.
        /// </summary>
        public TermCounterTester()
        {
            this.termRepositoryMock = new Mock<ISqlTermRepository>();
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
            LanguageCode languageCode = LanguageCode.ENGLISH;
            var userId = 1;
            var learningLevelDict = new Dictionary<string, LearningLevel>
            {
                { "hello", LearningLevel.Learning1 }, { "yolo", LearningLevel.Learning3 },
            };

            this.skippedWordRemoverMock.Setup(t => t.RemoveSkippedWords(words, languageCode))
                .Returns(words);
            this.textNormalizerMock.Setup(n => n.Normalize(words, languageCode))
                .Returns(words);
            this.termRepositoryMock
                .Setup(r => r.GetLearningLevelAsync(userId, languageCode, It.IsAny<HashSet<string>>()))
                .ReturnsAsync(learningLevelDict);

            Dictionary<LearningLevel, long> result =
                await this.termCounter.CountByLearningLevelAsync(words, languageCode, userId);
            Assert.Equal(
                LearningLevel.GetAll()
                    .Count(),
                result.Count);
            Assert.Equal(2, result[LearningLevel.Learning1]);
            Assert.Equal(1, result[LearningLevel.Learning3]);
            Assert.Equal(0, result[LearningLevel.Learning2]);
            Assert.Equal(0, result[LearningLevel.WellKnown]);
            Assert.Equal(1, result[LearningLevel.Unknown]);
        }
    }
}