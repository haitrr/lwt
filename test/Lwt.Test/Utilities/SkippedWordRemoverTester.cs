namespace Lwt.Test.Utilities
{
    using Lwt.Utilities;
    using Xunit;

    /// <summary>
    /// skipped word remover tester.
    /// </summary>
    public class SkippedWordRemoverTester
    {
        /// <summary>
        /// should get resolved by DI.
        /// </summary>
        [Fact]
        public void ShouldGetResolveByDi()
        {
            var helper = new DependencyResolverHelper();
            Assert.IsType<SkippedWordRemover>(helper.GetService<ISkippedWordRemover>());
        }
    }
}